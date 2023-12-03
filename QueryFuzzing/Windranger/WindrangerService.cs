using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QueryFuzzing.Joern;
using QueryFuzzing.Models;
using QueryFuzzing.Valgrind;
using QueryFuzzing.Valgrind.Models;
using QueryFuzzing.Windranger.Models;
using QueryFuzzingWebApp.Database;
using QueryFuzzingWebApp.Database.Models;
using System.IO.Compression;

namespace QueryFuzzing.Windranger
{
    public class WindrangerService : QueryFuzzBase, IQueryFuzzService
    {
        private readonly ILogger<QueryFuzzBase> _logger;
        private readonly IValgrindService _valgrindService;
        private QueryFuzzContext _db;

        public WindrangerService(ILogger<QueryFuzzBase> logger, IJoernService joernService, IValgrindService valgrindService, QueryFuzzContext db):base(logger, joernService, valgrindService, db)
        {
            _logger = logger;
            _valgrindService = valgrindService;
            _db = db;
        }

        public async Task<FuzzingInstance> PrepareFuzzing(int projectId)
        {
            _logger.LogInformation("Prepare Fuzzing");

            try
            {
                var project = await _db.Projects.SingleOrDefaultAsync(p => p.Id == projectId);
                if (project == null)
                {
                    return null;
                }

                var inst = await _db.FuzzingInstance.AddAsync(new FuzzingInstance
                {
                    ProjectId = projectId,
                    CreateDate = DateTime.Now,

                });
                await _db.SaveChangesAsync();
                string dockername = $"{project.Name}_{inst.Entity.Id}_{inst.Entity.CreateDate:ddMMyyyyHHmmss}";
                var lastFolderName = Path.GetFileName(
                        project.Path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

                List<string> output;
                _logger.LogInformation("Starting Docker");
                //Create Docker
                output = DockerExecuter.ExecDockerCommand($"run -itd --name {dockername} ardu/windranger", 12000);
                output = DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"apt-get update\"");
                output = DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"apt-get install valgrind\"");

                //Create Folder
                output = DockerExecuter.ExecDockerCommand($"exec {dockername} mkdir /home/SVF-tools/fuzzing");
                //Copy Project to Docker
                output = DockerExecuter.ExecDockerCommand($"cp {project.Path} {dockername}:/home/SVF-tools/fuzzing");
                output = DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"/home/SVF-tools/fuzzing/{lastFolderName}/prepare.sh \"");
                output = DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"find /home/SVF-tools/fuzzing/{lastFolderName}/build -executable -type f\"");
                var executables = GetExecutables(inst.Entity.Id, lastFolderName, output);

                await _db.Executables.AddRangeAsync(executables);
                await _db.SaveChangesAsync();

                return inst.Entity;
            }
            catch (Exception e)
            {
                _logger.LogError(e.StackTrace);
                throw;
            }

        }

        public async Task StartFuzzing(int instanceId)
        {
            var inst = await _db.FuzzingInstance.Include(i => i.Project).SingleOrDefaultAsync(i => i.Id == instanceId);
            if (inst == null)
            {
                return;
            }

            string dockername = $"{inst.Project.Name}_{inst.Id}_{inst.CreateDate:ddMMyyyyHHmmss}";
            var lastFolderName = Path.GetFileName(
                    inst.Project.Path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

            TargetCreator.CreateTargetFile(inst.InstanceTargets.ToList(), "Temp/targets");

            DockerExecuter.ExecDockerCommand($"exec {dockername} mkdir /home/SVF-tools/fuzzing/{lastFolderName}/build/fuzz");
            DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"~/gllvm/get-bc ~/fuzzing/{lastFolderName}/build{inst.SelectedExecutable.Path}\"");
            DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"cp ~/fuzzing/{lastFolderName}/build{inst.SelectedExecutable.Path}.bc ~/fuzzing/{lastFolderName}/build/fuzz/{inst.SelectedExecutable.Name}.bc\"");

            DockerExecuter.ExecDockerCommand($"cp Temp/targets {dockername}:/home/SVF-tools/fuzzing/{lastFolderName}/build/fuzz");
            DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"cd /home/SVF-tools/fuzzing/{lastFolderName}/build/fuzz; ~/windranger/instrument/bin/cbi --targets=targets {inst.SelectedExecutable.Name}.bc \"");
            DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"cd /home/SVF-tools/fuzzing/{lastFolderName}/build/fuzz; ~/windranger/fuzz/afl-clang-fast {inst.SelectedExecutable.Name}.ci.bc -lpng16 -lm -lz -lfreetype -o {inst.SelectedExecutable.Name}.ci \"");
            DockerExecuter.ExecDockerCommand($"exec {dockername} mkdir /home/SVF-tools/fuzzing/{lastFolderName}/build/fuzz/in");
            DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"cd /home/SVF-tools/fuzzing/{lastFolderName}/build/fuzz/in && echo > empty.txt \"");
            DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"cd /home/SVF-tools/fuzzing/{lastFolderName}/build/fuzz; AFL_I_DONT_CARE_ABOUT_MISSING_CRASHES=1 AFL_SKIP_CPUFREQ=1 ~/windranger/fuzz/afl-fuzz -d -i in/ -o out ./{inst.SelectedExecutable.Name}.ci @@ \"", 12000);
            File.Delete("Temp/targets");

        }

        public async Task<FuzzingStat> GetFuzzingStatus(int instanceId)
        {
            var inst = await _db.FuzzingInstance.Include(i => i.Project).SingleOrDefaultAsync(i => i.Id == instanceId);
            if (inst == null)
            {
                return null;
            }

            string dockername = $"{inst.Project.Name}_{inst.Id}_{inst.CreateDate:ddMMyyyyHHmmss}";
            var lastFolderName = Path.GetFileName(
                    inst.Project.Path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

            DockerExecuter.ExecDockerCommand($"cp {dockername}:/home/SVF-tools/fuzzing/{lastFolderName}/build/fuzz/out/fuzzer_stats Temp");
            var lines = File.ReadLines("Temp/fuzzer_stats");
            var status = await CreateFuzzingStats(lines);
            File.Delete("Temp/fuzzer_stats");
            return status;
        }

        public async Task<FuzzingResult> FinishFuzzing(int instanceId)
        {
            try
            {
                var inst = await _db.FuzzingInstance.Include(i => i.FinalStats).Include(i => i.SelectedExecutable).Include(i => i.Project).Include(i => i.InstanceTargets).Include(i => i.Crashes).ThenInclude(c => c.Stacktrace).Include(i => i.Crashes).ThenInclude(c => c.CrashedTargets).SingleOrDefaultAsync(i => i.Id == instanceId);
                if (inst == null)
                {
                    throw new Exception();
                }

                if (inst.EndTime > inst.StartTime)
                {
                    var result = new FuzzingResult
                    {
                        FalsePositives = inst.InstanceTargets.Where(it => !it.Crashed).ToList(),
                        Crashes = inst.Crashes.ToList(),
                        Status = inst.FinalStats
                    };
                    return result;
                }

                var status = await GetFuzzingStatus(instanceId);
                inst.FinalStats = status;
                _db.FuzzingInstance.Update(inst);
                await _db.SaveChangesAsync();
                var fuzzingResult = new FuzzingResult
                {
                    Status = status
                };

                string dockername = $"{inst.Project.Name}_{inst.Id}_{inst.CreateDate:ddMMyyyyHHmmss}";
                var lastFolderName = Path.GetFileName(
                        inst.Project.Path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));


                DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"kill {status.fuzzer_pid}\"");
                DockerExecuter.ExecDockerCommand($"cp Ressources/valgrind_analysis.sh {dockername}:/home/SVF-tools/fuzzing/{lastFolderName}/build/fuzz");
                var t = DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"cd /home/SVF-tools/fuzzing/{lastFolderName}/build/fuzz; ./valgrind_analysis.sh {inst.SelectedExecutable.Path}\"");

                DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"cd /home/SVF-tools/fuzzing/{lastFolderName}/build; zip -r fuzz_{dockername}.zip fuzz\"");
                DockerExecuter.ExecDockerCommand($"cp {dockername}:/home/SVF-tools/fuzzing/{lastFolderName}/build/fuzz_{dockername}.zip Temp");


                var loggedCrashes = new List<LoggedCrash>();
                var loggedTargets = new List<LoggedTarget>();
                var instrumentedTargets = new List<InstrumentedTarget>();
                var valgrindErrorList = new List<ValgrindError>();
                using (var file = File.OpenRead($"Temp/fuzz_{dockername}.zip"))
                using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
                {

                    foreach (var entry in zip.Entries.Where(e => e.FullName.StartsWith("fuzz/out/crashes/")))
                    {
                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            continue;
                        }
                        try
                        {
                            var crashTime = new DateTime(entry.LastWriteTime.Ticks, DateTimeKind.Utc).ToLocalTime();
                            var crashLogEntry = zip.Entries.SingleOrDefault(e => e.FullName.StartsWith($"fuzz/out/valgrind/crash_log_{entry.Name}"));
                            if (crashLogEntry != null)
                            {
                                using (var stream = crashLogEntry.Open())
                                {

                                    valgrindErrorList.AddRange(await _valgrindService.ParseValgrindLog(stream, crashTime.Subtract(status.start_time)));
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                    }
                }

                DockerExecuter.ExecDockerCommand($"stop {dockername}");
                inst.EndTime = DateTime.Now;
                _db.FuzzingInstance.Update(inst);
                await _db.SaveChangesAsync();
                var valgrindEqual = new ValgrindErrorEquals();

                foreach (var crash in valgrindErrorList.Distinct(valgrindEqual).OrderBy(v => v.FuzzingCrashTime))
                {
                    var dbCrash = new Crash
                    {
                        FuzzingInstanceId = inst.Id,
                        CrashTime = crash.FuzzingCrashTime,
                        Description = crash.ErrorMessage,
                        Stacktrace = crash.Stacktrace.Select(s => new Stacktrace { LineNumber = s.LineNumber, Method = s.Method, ClassName = s.ClassName, GlobalLib = s.GlobalLib }).ToList()
                    };
                    var crashedTargets = new List<CrashedTarget>();
                    foreach (var trace in crash.Stacktrace.Where(s => !s.GlobalLib).OrderBy(s => s.Id))
                    {
                        var crashedTarget = inst.InstanceTargets.Where(t => t.File == trace.ClassName && t.Methodname == trace.Method && t.Line == trace.LineNumber).ToList();
                        if (crashedTarget.Any())
                        {
                            _db.UpdateRange(crashedTarget.Select(c => { c.Crashed = true; return c; }).ToList());
                            await _db.SaveChangesAsync();
                            crashedTargets.AddRange(crashedTarget.Select(c => new CrashedTarget { Line = c.Line, Methodname = c.Methodname, File = c.File, QueryDescription = c.QueryDescription, Path = c.Path, MatchAccuracy = MatchAccuracy.Line }));
                        }
                        else
                        {
                            var methodCrash = inst.InstanceTargets.Where(t => t.File == trace.ClassName && t.Methodname == trace.Method).ToList();
                            if (methodCrash.Any())
                            {
                                _db.UpdateRange(crashedTarget.Select(c => { c.Crashed = true; return c; }).ToList());
                                await _db.SaveChangesAsync(); crashedTargets.AddRange(crashedTarget.Select(c => new CrashedTarget { Line = 0, Methodname = c.Methodname, File = c.File, QueryDescription = c.QueryDescription, Path = c.Path, MatchAccuracy = MatchAccuracy.Method }));
                            }
                        }
                    }
                    dbCrash.CrashedTargets = crashedTargets;

                    _db.Crash.Add(dbCrash);
                    await _db.SaveChangesAsync();
                }

                fuzzingResult.FalsePositives = inst.InstanceTargets.Where(it => !it.Crashed).ToList();
                fuzzingResult.Crashes = _db.Crash.Include(c => c.CrashedTargets).Where(c => c.FuzzingInstanceId == instanceId).ToList();
                return fuzzingResult;
            }
            catch (Exception ex)
            {
                return new FuzzingResult
                {
                    Crashes = new List<Crash>(),
                    FalsePositives = new List<InstanceTarget>(),
                    Status = new FuzzingStat()
                };
            }
        }

        private static List<Executable> GetExecutables(int instanceId, string workingDir, List<string> filepaths)
        {
            return filepaths.Select(f => new Executable
            {
                FuzzingInstanceId = instanceId,
                Path = f.Split($"build").Last(),
                Name = Path.GetFileName(
                    f.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            }
            ).ToList();
        }
        private async Task<FuzzingStat> CreateFuzzingStats(IEnumerable<string> lines)
        {
            var status = new FuzzingStat();
            foreach (var line in lines)
            {
                var valuePair = line.Split(':');
                switch (valuePair[0].Trim())
                {
                    case "start_time":
                        status.start_time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(int.Parse(valuePair[1].Trim())).ToLocalTime();
                        break;
                    case "last_update":
                        status.last_update = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(int.Parse(valuePair[1].Trim())).ToLocalTime();
                        break;
                    case "fuzzer_pid":
                        status.fuzzer_pid = int.Parse(valuePair[1].Trim());
                        break;
                    case "cycles_done":
                        status.cycles_done = int.Parse(valuePair[1].Trim());
                        break;
                    case "execs_done":
                        status.execs_done = int.Parse(valuePair[1].Trim());
                        break;
                    case "execs_per_sec":
                        status.execs_per_sec = float.Parse(valuePair[1].Trim());
                        break;
                    case "paths_total":
                        status.paths_total = int.Parse(valuePair[1].Trim());
                        break;
                    case "paths_favored":
                        status.paths_favored = int.Parse(valuePair[1].Trim());
                        break;
                    case "paths_found":
                        status.paths_found = int.Parse(valuePair[1].Trim());
                        break;
                    case "paths_imported":
                        status.paths_imported = int.Parse(valuePair[1].Trim());
                        break;
                    case "max_depth":
                        status.max_depth = int.Parse(valuePair[1].Trim());
                        break;
                    case "cur_path":
                        status.cur_path = int.Parse(valuePair[1].Trim());
                        break;
                    case "pending_favs":
                        status.pending_favs = int.Parse(valuePair[1].Trim());
                        break;
                    case "pending_total":
                        status.pending_total = int.Parse(valuePair[1].Trim());
                        break;
                    case "variable_paths":
                        status.variable_paths = int.Parse(valuePair[1].Trim());
                        break;
                    case "stability":
                        status.stability = valuePair[1].Trim();
                        break;
                    case "bitmap_cvg":
                        status.bitmap_cvg = valuePair[1].Trim();
                        break;
                    case "unique_crashes":
                        status.unique_crashes = int.Parse(valuePair[1].Trim());
                        break;
                    case "unique_hangs":
                        status.unique_hangs = int.Parse(valuePair[1].Trim());
                        break;
                    case "last_path":
                        status.last_path = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(int.Parse(valuePair[1].Trim())).ToLocalTime();
                        break;
                    case "last_crash":
                        status.last_crash = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(int.Parse(valuePair[1].Trim())).ToLocalTime();
                        break;
                    case "last_hang":
                        status.last_hang = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(int.Parse(valuePair[1].Trim())).ToLocalTime();
                        break;
                    default: break;
                }

            }
            return status;
        }
    }
}
