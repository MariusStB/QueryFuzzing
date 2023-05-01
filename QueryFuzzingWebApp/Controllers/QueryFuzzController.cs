using Microsoft.AspNetCore.Mvc;
using QueryFuzzingWebApp.Services;
using QueryFuzzingWebApp.Models;
using QueryFuzzingWebApp.Database;
using Microsoft.EntityFrameworkCore;
using QueryFuzzingWebApp.Database.Models;

namespace QueryFuzzingWebApp.Controllers
{
    public class QueryFuzzController :Controller
    {
        private readonly ILogger<QueryFuzzController> _logger;
        private readonly IQueryFuzzService _queryFuzzService;
        private readonly QueryFuzzContext _db;

        public QueryFuzzController(ILogger<QueryFuzzController> logger, IQueryFuzzService queryFuzzService, QueryFuzzContext db)
        {
            _logger = logger;   
            _queryFuzzService = queryFuzzService;
            _db = db;
        }

        public IActionResult Index()
        {
            var projects = _db.Projects.ToList();
            return View(projects);
        }

        public IActionResult Start()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> StartAnalysis(QueryFuzzStartAnalysisModel model)
        {
            var project = await _queryFuzzService.ExecuteQueryAnalysis(model);
            if(project == null)
            {
                return View("Index");
            }
            return View("Targets", project);
        }

        public async Task<IActionResult> TargetsAsync(int projectId)
        {
            var project = await _db.Projects.Include(p => p.Targets).SingleOrDefaultAsync(p => p.Id == projectId);
            if (project == null)
            {
                return View("Index");
            }
            return View(project);
        }

        [HttpPost]
        public async Task<IActionResult> StartFuzzing([FromForm] QueryFuzzTargetExecutableModel model)
        {
            var inst = await _db.FuzzingInstance.SingleOrDefaultAsync(i => i.Id == model.SelectedInstance);
            if(inst == null)
            {
                return View("Index");
            }

            inst.SelectedExecutable = _db.Executables.Single(e => e.Id == model.SelectedExecutable);
            inst.InstanceTargets = _db.Targets.Where(t => model.Targets.Contains(t.Id)).Select(t => new InstanceTarget {
                    File= t.File,
                    Line= t.Line,
                }).ToList();

            _db.FuzzingInstance.Update(inst);
            await _db.SaveChangesAsync();
            await _queryFuzzService.StartFuzzing(inst.Id);
            return View("Index");
        }

        public async Task<IActionResult> CreateFuzzingInstance(int projectId)
        {            
            var instance = await _queryFuzzService.PrepareFuzzing(projectId);
            var model = new QueryFuzzTargetExecutableModel
            {
                Project = await _db.Projects.Include(p => p.Targets).Include(p => p.FuzzingInstance).SingleOrDefaultAsync(p => p.Id == projectId),
                SelectedInstance = instance.Id
            };
            return View("SelectTargets", model);
        }
    }
}
