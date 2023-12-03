using QueryFuzzing.Joern.Models;

namespace QueryFuzzing.Models
{
    public class QueryFuzzStartAnalysisModel
    {

        public string ProjectName { get; set; }
        public string ProjectPath { get; set; }
        public Language Language { get; set; }
        public Tag Tag { get; set; }

    }
}
