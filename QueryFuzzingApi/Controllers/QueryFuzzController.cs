using Microsoft.AspNetCore.Mvc;
using QueryFuzzing.Joern.Models;
using QueryFuzzing.Joern;
using QueryFuzzing.TargetFuzzing;
using QueryFuzzingApi.Models;
using System.Reflection;

namespace QueryFuzzingApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueryFuzzController : ControllerBase
    {
        private readonly ILogger<QueryFuzzController> _logger;
        private readonly IJoernService _joernService;

        public QueryFuzzController(ILogger<QueryFuzzController> logger, IJoernService joernService)
        {
            _logger = logger;
            _joernService = joernService;
        }      

        [HttpPost("ExecuteQueryAnalysis")]
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
                        foreach (var r in queryResult)
                        {
                            matchList.AddRange(QueryListParser.ParseCallList(r));
                        }
                    }

                }
                var file= TargetCreator.CreateTargetFile(matchList, $"{model.TargetPath}\\TargetsBB{DateTime.Now:ddMMyyyyHHmmss}.txt");

                return new QueryAnalysisResult
                {
                    QueryCallMatches = matchList,
                    TargetFile = file
                };

            }
            _logger.LogInformation("Query Analysis failed");
            return null;
            
        }


        [HttpGet("ShowQueries")]
        public async Task<List<string>> ShowQueries(Language language = Language.c, Tag tag = Tag.none)
        {
            return await _joernService.GetQueryDbItems(language, tag);
        }
    }
}