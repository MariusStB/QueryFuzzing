using QueryFuzzing.Joern;
using QueryFuzzingWebApp.Database;
using QueryFuzzingWebApp.Database.Models;
using QueryFuzzing.Windranger;
using QueryFuzzing.Windranger.Models;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using LoggedTarget = QueryFuzzing.Windranger.Models.LoggedTarget;
using QueryFuzzing.Valgrind;
using QueryFuzzing.Models;
using QueryFuzzing.Valgrind.Models;
using Microsoft.Extensions.Logging;

namespace QueryFuzzing
{
    public class QueryFuzzBase
    {
        private readonly ILogger<QueryFuzzBase> _logger;
        private readonly IJoernService _joernService;
        private readonly IValgrindService _valgrindService;
        private QueryFuzzContext _db;

        public QueryFuzzBase(ILogger<QueryFuzzBase> logger, IJoernService joernService, IValgrindService valgrindService, QueryFuzzContext db)
        {
            _logger = logger;
            _joernService = joernService;
            _valgrindService = valgrindService;
            _db = db;
        }


        public async Task<Project> ExecuteQueryAnalysis(QueryFuzzStartAnalysisModel model)
        {
            try
            {
                _logger.LogInformation("Start Query Analysis");
                var startDate = DateTime.Now;

                var queries = await _joernService.GetQueryDbItems(model.Language, model.Tag);


                if (await _joernService.ImportProject(model.ProjectName, model.ProjectPath))
                {
                    var matchList = new List<ProjectTarget>();
                    foreach (var query in queries)
                    {
                        var queryResult = await _joernService.SendQuery(query.Traversal);
                        if (queryResult.Any(r => !string.IsNullOrEmpty(r)))
                        {
                            int c = queryResult.Count();
                            foreach (var r in queryResult)
                            {
                                matchList.AddRange(QueryListParser.ParseCallList(r).Select(m => new ProjectTarget { File = Path.GetFileName(m.Path), Path = m.Path, Line = m.LineNumber, Methodname = m.Methodname, QueryDescription = query.Description }));
                            }
                        }

                    }

                    var p = await _db.AddAsync(new Project
                    {
                        Name = model.ProjectName.Replace(" ", "_"),
                        Path = model.ProjectPath,
                        CreatedDate = startDate,
                        QueryTime = DateTime.Now.Subtract(startDate),
                        Language = model.Language,
                        Tag = model.Tag,
                        Targets = matchList

                    });
                    await _db.SaveChangesAsync();

                    return p.Entity;

                }
                _logger.LogInformation("Query Analysis failed");
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }       
       
    }
}
