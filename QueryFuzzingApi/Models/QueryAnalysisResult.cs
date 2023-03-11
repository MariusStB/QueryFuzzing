using QueryFuzzing.Joern.Models;

namespace QueryFuzzingApi.Models
{
    public class QueryAnalysisResult
    {
        public List<QueryCallMatch> QueryCallMatches { get; set; }
        public byte[] TargetFile { get; set; }
    }
}
