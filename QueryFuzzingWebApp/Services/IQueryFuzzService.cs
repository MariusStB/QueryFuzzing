using QueryFuzzing.Models;
using QueryFuzzingWebApp.Database.Models;
using QueryFuzzingWebApp.Models;

namespace QueryFuzzingWebApp.Services
{
    public interface IQueryFuzzService
    {
        //Task<string> ExecuteQueryFuzzing( model);
        Task<Project> ExecuteQueryAnalysis(QueryFuzzStartAnalysisModel model);
        Task<FuzzingInstance> PrepareFuzzing(int projectId);

        Task StartFuzzing(int instanceId);

        Task<FuzzingStat> GetFuzzingStatus(int instanceId);

        Task<FuzzingResult> FinishFuzzing(int instanceId);
    }

}
