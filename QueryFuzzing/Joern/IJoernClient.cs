using QueryFuzzing.Joern.Models;

namespace QueryFuzzing.Joern
{
    public interface IJoernClient
    {
        Task<QueryResponse> SubmitQuery(QueryRequest request);
        Task<ResultResponse> RetrieveResponse(string uuid);
    }
}
