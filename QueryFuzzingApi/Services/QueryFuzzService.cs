using QueryFuzzing.Joern.Models;
using QueryFuzzing.Joern;
using QueryFuzzing.TargetFuzzing;
using QueryFuzzingApi.Models;
using QueryFuzzingApi.Controllers;
using System.IO.Compression;
using QueryFuzzing.Models;

namespace QueryFuzzingApi.Services
{
    public class QueryFuzzService : IQueryFuzzService
    {
        private readonly ILogger<QueryFuzzController> _logger;
        private readonly IJoernService _joernService;
        private string workingPath = "C:\\Temp\\workspace";


        public QueryFuzzService(ILogger<QueryFuzzController> logger, IJoernService joernService)
        {
            _logger = logger;
            _joernService = joernService;   
        }


        public async Task<QueryAnalysisResult> ExecuteQueryAnalysis(ExecuteQueryAnalysisModel model)
        {
            _logger.LogInformation("Start Query Analysis");
            var queries = await _joernService.GetQueryDbItems(model.Language, model.Tag);


            if (await _joernService.ImportProject(model.ProjectName, model.ProjectPath))
            {
                var matchList = new List<QueryCallMatch>();
                foreach (var query in queries)
                {
                    var queryResult = await _joernService.SendQuery(query);
                    if (queryResult.Any(r => !string.IsNullOrEmpty(r)))
                    {
                       int c = queryResult.Count();
                        foreach (var r in queryResult)
                        {
                            matchList.AddRange(QueryListParser.ParseCallList(r));
                        }
                    }

                }
                var file = TargetCreator.CreateTargetFile(matchList, $"{model.TargetPath}\\TargetsBB{DateTime.Now:ddMMyyyyHHmmss}.txt");

                return new QueryAnalysisResult
                {
                    QueryCallMatches = matchList,
                    TargetFile = file
                };

            }
            _logger.LogInformation("Query Analysis failed");
            return null;
        }

        public async Task<string> ExecuteQueryFuzzing(ExecuteQueryFuzzingModel model)
        {
            try
            {
                string projectName = Path.GetFileNameWithoutExtension(model.RepositoryZipPath);
                ZipFile.ExtractToDirectory(model.RepositoryZipPath, workingPath, true);
                var joern = await ExecuteQueryAnalysis(new ExecuteQueryAnalysisModel
                {
                    ProjectPath = $"{workingPath}\\{projectName}",
                    ProjectName = projectName,
                    Language = Language.c,
                    Tag = Tag.none,
                    TargetPath = $"{workingPath}\\{projectName}\\Target"
                });

                return null;
            }
            catch (Exception ex)
            {   
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
    }
}
