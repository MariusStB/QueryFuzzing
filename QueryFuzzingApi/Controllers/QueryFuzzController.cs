using Microsoft.AspNetCore.Mvc;
using QueryFuzzing.Joern.Models;
using QueryFuzzing.Joern;
using QueryFuzzing.TargetFuzzing;
using QueryFuzzingApi.Models;
using System.Reflection;
using QueryFuzzingApi.Services;
using QueryFuzzing.Models;

namespace QueryFuzzingApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueryFuzzController : ControllerBase
    {
        private readonly ILogger<QueryFuzzController> _logger;
        private readonly IJoernService _joernService;
        private readonly IQueryFuzzService _queryFuzzService;

        public QueryFuzzController(ILogger<QueryFuzzController> logger, IJoernService joernService, IQueryFuzzService queryFuzzService)
        {
            _logger = logger;
            _joernService = joernService;
            _queryFuzzService = queryFuzzService;
        }

        [HttpPost("ExecuteQueryFuzzing")]
        public async Task<string> ExecuteQueryFuzzing([FromForm]ExecuteQueryFuzzingModel model)
        {
            _logger.LogInformation("Start Query Fuzzing");
            return await _queryFuzzService.ExecuteQueryFuzzing(model);

        }


        [HttpPost("ExecuteQueryAnalysis")]
        public async Task<QueryAnalysisResult> ExecuteQueryAnalysis(ExecuteQueryAnalysisModel model)
        {
           return await _queryFuzzService.ExecuteQueryAnalysis(model);
            
        }


        [HttpGet("ShowQueries")]
        public async Task<List<string>> ShowQueries(Language language = Language.c, Tag tag = Tag.none)
        {
            return await _joernService.GetQueryDbItems(language, tag);
        }
    }
}