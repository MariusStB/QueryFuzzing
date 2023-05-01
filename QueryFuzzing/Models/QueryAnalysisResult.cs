using QueryFuzzing.Joern.Models;

namespace QueryFuzzing.Models
{
    public class QueryAnalysisResult
    {
        public List<QueryCallMatch> QueryCallMatches { get; set; }
        public byte[] TargetFile { get; set; }
    }
}
