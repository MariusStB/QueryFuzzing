using QueryFuzzing.Joern.Models;
using QueryFuzzing.Joern;
using QueryFuzzing.TargetFuzzing;
using System.IO.Compression;
using QueryFuzzing.Models;
using QueryFuzzingWebApp.Models;
using QueryFuzzingWebApp.Database;
using QueryFuzzingWebApp.Database.Models;
using QueryFuzzing.Windranger;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace QueryFuzzingWebApp.Services
{
    public class QueryFuzzService : IQueryFuzzService
    {
        private readonly ILogger<QueryFuzzService> _logger;
        private readonly IJoernService _joernService;
        private string workingPath = "C:\\Temp\\workspace";
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

            List<string> output;
            output = DockerExecuter.ExecDockerCommand($"cp {project.Path} {dockername}:/home/SVF-tools/fuzzing");
        }

        //public async Task<string> ExecuteQueryFuzzing(ExecuteQueryFuzzingModel model)
        //{
        //    try
        //    {
        //        string projectName = Path.GetFileNameWithoutExtension(model.RepositoryZipPath);
        //        ZipFile.ExtractToDirectory(model.RepositoryZipPath, workingPath, true);
        //        var joern = await ExecuteQueryAnalysis(new ExecuteQueryAnalysisModel
        //        {
        //            ProjectPath = $"{workingPath}\\{projectName}",
        //            ProjectName = projectName,
        //            Language = Language.c,
        //            Tag = Tag.none,
        //            TargetPath = $"{workingPath}\\{projectName}\\Target"
        //        });

        //        return null;
        //    }
        //    catch (Exception ex)
        //    {   
        //        Console.WriteLine(ex.ToString());
        //        return null;
        //    }
        //}

        private static List<Executable> GetExecutables(int instanceId, string workingDir, List<string> filepaths)
        {
            return filepaths.Select(f => new Executable
            {
                FuzzingInstanceId= instanceId,
                Path = f.Split(workingDir).Last(),
                Name = Path.GetFileName(
                    f.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            }
            ).ToList();
        }

        
    }

   
}
