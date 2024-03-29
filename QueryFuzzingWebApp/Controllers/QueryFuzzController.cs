﻿using Microsoft.AspNetCore.Mvc;
using QueryFuzzingWebApp.Database;
using Microsoft.EntityFrameworkCore;
using QueryFuzzingWebApp.Database.Models;
using System.Runtime.CompilerServices;
using QueryFuzzing;
using QueryFuzzing.Models;
using QueryFuzzingWebApp.Models;

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
                return RedirectToAction("Index");
            }
            return View("Targets", project);
        }

        public async Task<IActionResult> TargetsAsync(int projectId)
        {
            var project = await _db.Projects.Include(p => p.Targets).SingleOrDefaultAsync(p => p.Id == projectId);
            if (project == null)
            {
                return RedirectToAction("Index");
            }
            return View(project);
        }

        [HttpPost]
        public async Task<IActionResult> StartFuzzing([FromForm] QueryFuzzTargetExecutableModel model)
        {
            var inst = await _db.FuzzingInstance.SingleOrDefaultAsync(i => i.Id == model.SelectedInstance);
            if(inst == null)
            {
                return RedirectToAction("Index");
            }

            inst.SelectedExecutable = _db.Executables.Single(e => e.Id == model.SelectedExecutable);
            inst.InstanceTargets = _db.ProjectTargets.Where(t => model.Targets.Contains(t.Id)).Select(t => new InstanceTarget {
                    File= t.File,
                    Path = t.Path,
                    Line= t.Line,
                    Methodname = t.Methodname,
                    QueryDescription= t.QueryDescription,
                }).ToList();

            _db.FuzzingInstance.Update(inst);
            await _db.SaveChangesAsync();
            await _queryFuzzService.StartFuzzing(inst.Id);

            return RedirectToAction("Status", new { instanceId = inst.Id });
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

        public async Task<IActionResult> FuzzingInstance(int projectId)
        {
            var project = await _db.Projects.Include(p => p.FuzzingInstance).ThenInclude(i => i.InstanceTargets).SingleOrDefaultAsync(p => p.Id == projectId);
            if(project == null)
            {
                return RedirectToAction("Index");
            }

            return View(project);
        }


        public async Task<IActionResult> Status(int instanceId)
        {
            var inst = await _db.FuzzingInstance.Include(i => i.Project).SingleOrDefaultAsync(i => i.Id == instanceId);
            if (inst == null)
            {
                return RedirectToAction("Index");
            }
            if (inst.EndTime> inst.StartTime)
            {
                var stats = await _db.FuzzingStats.SingleOrDefaultAsync(s => s.Id == inst.FinalStatId);
                return View(new QueryFuzzStatusModel
                {
                    Project= inst.Project,
                    SelectedInstance=inst.Id,
                    FuzzingStatus = stats
                });
            }
            var status = await _queryFuzzService.GetFuzzingStatus(instanceId);
            var model = new QueryFuzzStatusModel
            {
                Project = inst.Project,
                SelectedInstance = inst.Id,
                FuzzingStatus = status
            };

            return View(model);
        }

        public async Task<IActionResult> DeleteProject(int projectId)
        {
            var project = await _db.Projects.SingleOrDefaultAsync(i => i.Id == projectId);
            if (project == null)
            {
                return RedirectToAction("Index");
            }

            _db.Remove<Project>(project);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteInstance(int instanceId)
        {
            var inst = await _db.FuzzingInstance.SingleOrDefaultAsync(i => i.Id == instanceId);
            if (inst == null)
            {
                return RedirectToAction("Index");
            }

            _db.Remove<FuzzingInstance>(inst);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Finish(int instanceId)
        {
            var inst = await _db.FuzzingInstance.Include(i => i.Project).SingleOrDefaultAsync(i => i.Id == instanceId);
            if (inst == null)
            {
                return RedirectToAction("Index");
            }

            var result = await _queryFuzzService.FinishFuzzing(instanceId);
            var model = new QueryFuzzResultModel
            {
                Project = inst.Project,
                SelectedInstance = inst.Id,
                FuzzingResult = result
            };                       

            return View(model);
        }

        
    }
}
