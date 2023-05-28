using QueryFuzzing.Joern;
using QueryFuzzingWebApp.Models;
using QueryFuzzingWebApp.Database;
using QueryFuzzingWebApp.Database.Models;
using QueryFuzzing.Windranger;
using QueryFuzzing.Windranger.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using System.IO.Compression;
using LoggedTarget = QueryFuzzing.Windranger.Models.LoggedTarget;

namespace QueryFuzzingWebApp.Services
{
    public class QueryFuzzService : IQueryFuzzService
    {
        private readonly ILogger<QueryFuzzService> _logger;
        private readonly IJoernService _joernService;
        private QueryFuzzContext _db;


        public QueryFuzzService(ILogger<QueryFuzzService> logger, IJoernService joernService, QueryFuzzContext db)
        {
            _logger = logger;
            _joernService = joernService;   
            _db = db;
        }


        public async Task<Project> ExecuteQueryAnalysis(QueryFuzzStartAnalysisModel model)
        {
            try { 
                _logger.LogInformation("Start Query Analysis");
               

                var queries = await _joernService.GetQueryDbItems(model.Language, model.Tag);


                if (await _joernService.ImportProject(model.ProjectName, model.ProjectPath))
                {
                    var matchList = new List<ProjectTarget>();
                    foreach (var query in queries)
                    {
                        var queryResult = await _joernService.SendQuery(query);
                        if (queryResult.Any(r => !string.IsNullOrEmpty(r)))
                        {
                           int c = queryResult.Count();
                            foreach (var r in queryResult)
                            {
                                matchList.AddRange(QueryListParser.ParseCallList(r).Select(m => new ProjectTarget { File = m.Filename, Line = m.LineNumber}));
                            }
                        }

                    }

                    var p = await _db.AddAsync(new Project
                    {
                        Name = model.ProjectName,
                        Path = model.ProjectPath,
                        CreatedDate = DateTime.Now,
                        Language = model.Language,
                        Tag = model.Tag,
                        Targets= matchList

                    });
                    await _db.SaveChangesAsync();

                    return p.Entity;

                }
                _logger.LogInformation("Query Analysis failed");
                return null;
            }
            catch (Exception ex) {
                return null;
            }
        }

        public async Task<FuzzingInstance> PrepareFuzzing(int projectId)
        {
            var project = await _db.Projects.SingleOrDefaultAsync(p => p.Id== projectId);
            if(project == null)
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
            //Create Docker
            //docker run --name fuzzing_02 -it ardu/windranger
            output = DockerExecuter.ExecDockerCommand($"run --name {dockername} -it ardu/windranger", 12000);
            output = DockerExecuter.ExecDockerCommand($"cp Ressources/afl-fuzz.c {dockername}:/home/SVF-tools/windranger/fuzz");
            output = DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"cd /home/SVF-tools/windranger/fuzz;make \"", 12000);



            //Create Folder
            //docker exec fuzzing_02 mkdir /home/SVF-tools/fuzzing
            output = DockerExecuter.ExecDockerCommand($"exec {dockername} mkdir /home/SVF-tools/fuzzing");
            //Copy Project to Docker
            //docker cp C:\Users\mstur\OneDrive\Desktop\share\libming-CVE-2018-8807\libming-CVE-2018-8807 fuzzing_02:/home/SVF-tools/fuzzing
            output = DockerExecuter.ExecDockerCommand($"cp {project.Path} {dockername}:/home/SVF-tools/fuzzing");
            output = DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"/home/SVF-tools/fuzzing/{lastFolderName}/prepare.sh \"");
            output = DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"find /home/SVF-tools/fuzzing/{lastFolderName}/build -executable -type f\"");
            var executables = GetExecutables(inst.Entity.Id,lastFolderName, output);

            await _db.Executables.AddRangeAsync(executables);
            await _db.SaveChangesAsync();
           


            return inst.Entity;            

        }

        public async Task StartFuzzing(int instanceId)
        {
            var inst = await _db.FuzzingInstance.Include(i => i.Project).SingleOrDefaultAsync(i => i.Id== instanceId);
            if(inst == null)
            {
                return;
            }

            string dockername = $"{inst.Project.Name}_{inst.Id}_{inst.CreateDate:ddMMyyyyHHmmss}";
            var lastFolderName = Path.GetFileName(
                    inst.Project.Path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

            TargetCreator.CreateTargetFile(inst.InstanceTargets.ToList(), "Temp/targets");


            List<string> output;
            output = DockerExecuter.ExecDockerCommand($"exec {dockername} mkdir /home/SVF-tools/fuzzing/{lastFolderName}/build/fuzz");
            output = DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"~/gllvm/get-bc ~/fuzzing/{lastFolderName}/build{inst.SelectedExecutable.Path}\"");
            output = DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"cp ~/fuzzing/{lastFolderName}/build{inst.SelectedExecutable.Path}.bc ~/fuzzing/{lastFolderName}/build/fuzz/{inst.SelectedExecutable.Name}.bc\"");

            output = DockerExecuter.ExecDockerCommand($"cp Temp/targets {dockername}:/home/SVF-tools/fuzzing/{lastFolderName}/build/fuzz");
            output = DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"cd /home/SVF-tools/fuzzing/{lastFolderName}/build/fuzz; ~/windranger/instrument/bin/cbi --targets=targets {inst.SelectedExecutable.Name}.bc \"");
            output = DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"cd /home/SVF-tools/fuzzing/{lastFolderName}/build/fuzz; ~/windranger/fuzz/afl-clang-fast {inst.SelectedExecutable.Name}.ci.bc -lpng16 -lm -lz -lfreetype -o {inst.SelectedExecutable.Name}.ci \"");
            output = DockerExecuter.ExecDockerCommand($"exec {dockername} mkdir /home/SVF-tools/fuzzing/{lastFolderName}/build/fuzz/in");
            output = DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"cd /home/SVF-tools/fuzzing/{lastFolderName}/build/fuzz/in && echo > empty.txt \"");
            output = DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"cd /home/SVF-tools/fuzzing/{lastFolderName}/build/fuzz; AFL_I_DONT_CARE_ABOUT_MISSING_CRASHES=1 AFL_SKIP_CPUFREQ=1 ~/windranger/fuzz/afl-fuzz -d -i in/ -o out ./{inst.SelectedExecutable.Name}.ci @@ \"",12000);


        }

        public async Task<FuzzingStatus> GetFuzzingStatus(int instanceId)
        {
            var inst = await _db.FuzzingInstance.Include(i => i.Project).SingleOrDefaultAsync(i => i.Id == instanceId);
            if (inst == null)
            {
                return null;
            }

            string dockername = $"{inst.Project.Name}_{inst.Id}_{inst.CreateDate:ddMMyyyyHHmmss}";
            var lastFolderName = Path.GetFileName(
                    inst.Project.Path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
           
            List<string> output;
            output = DockerExecuter.ExecDockerCommand($"cp {dockername}:/home/SVF-tools/fuzzing/{lastFolderName}/build/fuzz/out/fuzzer_stats Temp");
            var lines = File.ReadLines("Temp/fuzzer_stats");
            var status = new FuzzingStatus();
            foreach (var line in lines)
            {
                var valuePair = line.Split(':');
                switch (valuePair[0].Trim()) {
                case "start_time":
                        status.start_time = new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc).AddSeconds(int.Parse(valuePair[1].Trim()));
                        break;
                case "last_update":
                        status.last_update = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(int.Parse(valuePair[1].Trim()));
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
                        status.last_path = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(int.Parse(valuePair[1].Trim()));
                        break;
                case "last_crash":
                        status.last_crash = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(int.Parse(valuePair[1].Trim()));
                        break;
                case "last_hang":
                        status.last_hang = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(int.Parse(valuePair[1].Trim()));
                        break;
                default: break;
                }

            }
            File.Delete("Temp/fuzzer_stats");
            return status;
        }


        private static List<Executable> GetExecutables(int instanceId, string workingDir, List<string> filepaths)
        {
            return filepaths.Select(f => new Executable
            {
                FuzzingInstanceId= instanceId,
                Path = f.Split($"build").Last(),
                Name = Path.GetFileName(
                    f.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            }
            ).ToList();
        }

        public async Task<FuzzingResult> FinishFuzzing(int instanceId)
        {
#if (true)
            var inst = await _db.FuzzingInstance.Include(i => i.Project).SingleOrDefaultAsync(i => i.Id == instanceId);
            if (inst == null)
            {
                return null;
            }
            string dockername = $"{inst.Project.Name}_{inst.Id}_{inst.CreateDate:ddMMyyyyHHmmss}";
            var lastFolderName = Path.GetFileName(
                    inst.Project.Path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

            List<string> output;
            output = DockerExecuter.ExecDockerCommand($"exec {dockername} bash -c \"cd /home/SVF-tools/fuzzing/{lastFolderName}/build; zip -r fuzz.zip fuzz\"");
            output = DockerExecuter.ExecDockerCommand($"cp {dockername}:/home/SVF-tools/fuzzing/{lastFolderName}/build/fuzz.zip Temp");
#endif
            //byte[] byteArray = Encoding.ASCII.GetBytes(test);
            //MemoryStream stream = new MemoryStream(byteArray);
            using (var file = File.OpenRead("Temp/fuzz.zip"))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
            {
                ZipArchiveEntry e = zip.GetEntry("fuzz/out/crashes.log");
                using (var stream = e.Open())
                {
                   var crashes =  ParseLoggedCrashes(stream);
                }

                e = zip.GetEntry("fuzz/out/targets.log");
                using (var stream = e.Open())
                {
                    var targets = ParseLoggedTargets(stream);
                }

                e = zip.GetEntry("fuzz/targets.txt");
                using (var stream = e.Open())
                {
                    var targets = ParseInstrumentedTargets(stream);
                }

            }


            return null;
        }

        private static List<LoggedCrash> ParseLoggedCrashes(Stream stream)
        {
            try
            {
                // convert stream to string
                StreamReader reader = new StreamReader(stream);
                var regex = new Regex(@"(?<id>[0-9]+)\s:\s(?<days>[0-9]+)\sdays,\s(?<hrs>[0-9]+)\shrs,\s(?<min>[0-9]+)\smin,\s(?<sec>[0-9]+)\ssec\s---\s(?<exec>[^""]*)"
                   , RegexOptions.IgnorePatternWhitespace);
                var crashList = new List<LoggedCrash>();
                while (!reader.EndOfStream)
                {                    
                    var m = regex.Match(reader.ReadLine()??"");
                    if(m.Success) {
                        crashList.Add(new LoggedCrash
                        {
                            Id = int.Parse(m.Groups["id"].Value),
                            Days = int.Parse(m.Groups["days"].Value),
                            Hours = int.Parse(m.Groups["hrs"].Value),
                            Minutes = int.Parse(m.Groups["min"].Value),
                            Seconds = int.Parse(m.Groups["sec"].Value),
                            Execution = m.Groups["exec"].Value
                        });
                    }                   
                    
                }

                return crashList;

            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return new List<LoggedCrash>();
            }
           
        }

        private static List<LoggedTarget> ParseLoggedTargets(Stream stream)
        {
            try
            {
                // convert stream to string
                StreamReader reader = new StreamReader(stream);
                var regex = new Regex(@"(?<id>[0-9]+)\s:\s(?<days>[0-9]+)\sdays,\s(?<hrs>[0-9]+)\shrs,\s(?<min>[0-9]+)\smin,\s(?<sec>[0-9]+)\ssec\s---\s(?<queue>[0-9]+)\s---\s(?<exec>[^""]*)"
                   , RegexOptions.IgnorePatternWhitespace);
                var targetList = new List<LoggedTarget>();
                while (!reader.EndOfStream)
                {
                    var m = regex.Match(reader.ReadLine() ?? "");
                    if (m.Success)
                    {
                        targetList.Add(new LoggedTarget
                        {
                            Id = int.Parse(m.Groups["id"].Value),
                            Days = int.Parse(m.Groups["days"].Value),
                            Hours = int.Parse(m.Groups["hrs"].Value),
                            Minutes = int.Parse(m.Groups["min"].Value),
                            Seconds = int.Parse(m.Groups["sec"].Value),
                            Queue = int.Parse(m.Groups["queue"].Value),
                            Execution = m.Groups["exec"].Value
                        });
                    }
                    
                }

                return targetList;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new List<LoggedTarget>();
            }

        }

        private static List<InstrumentedTarget> ParseInstrumentedTargets(Stream stream)
        {
            try
            {
                // convert stream to string
                StreamReader reader = new StreamReader(stream);
                //(cl:\s(?<column>[0-9]+)\s)?fl:\s(?<file>[^""]*)
                var regex = new Regex(@"(?<id>[0-9]+)\s\sln:\s(?<line>[0-9]+)\s(\scl:\s(?<column>[0-9]+)\s\s)?(?<file>[^""]*)"
                   , RegexOptions.IgnorePatternWhitespace);
                var regex2 = new Regex(@"(?<id>[0-9]+)\s:\s(?<days>[0-9]+)\sdays,\s(?<hrs>[0-9]+)\shrs,\s(?<min>[0-9]+)\smin,\s(?<sec>[0-9]+)\ssec\s---\s(?<queue>[0-9]+)\s---\s(?<exec>[^""]*)"
   , RegexOptions.IgnorePatternWhitespace);
                var targetList = new List<InstrumentedTarget>();
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine().Replace("{", "").Replace("}", "").Trim();
                    var m = regex.Match(line ?? "");
                    if (m.Success)
                    {
                        targetList.Add(new InstrumentedTarget
                        {
                            Id = int.Parse(m.Groups["id"].Value),
                            Line = int.Parse(m.Groups["line"].Value),
                            Column = !string.IsNullOrEmpty(m.Groups["column"].Value) ? int.Parse(m.Groups["column"].Value) : 0,
                            File = m.Groups["file"].Value
                        });
                    }else
                    {
                        Console.WriteLine("error");
                    }
                    
                }
                return targetList;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new List<InstrumentedTarget>();
            }

        }
    }

   
}
