using QueryFuzzing.Models;
using QueryFuzzingApi.Models;

namespace QueryFuzzingApi.Services
{
    public interface IQueryFuzzService
    {
        Task<string> ExecuteQueryFuzzing(ExecuteQueryFuzzingModel model);
        Task<QueryAnalysisResult> ExecuteQueryAnalysis(ExecuteQueryAnalysisModel model);
    }
}
