using QueryFuzzing.Joern.Models;
using System.ComponentModel;

namespace QueryFuzzingWebApp.Models
{
    public class QueryFuzzStartAnalysisModel
    {

        [DefaultValue("libming-CVE-2018-8807")]
        public string ProjectName { get; set; }
        [DefaultValue("/home/marius/temp/libming-CVE-2018-8807")]
        public string ProjectPath { get; set; }
        public Language Language { get; set; }
        public Tag Tag { get; set; }
      
    }
}
