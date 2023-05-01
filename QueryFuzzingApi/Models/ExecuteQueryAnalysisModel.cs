using QueryFuzzing.Joern.Models;
using System.ComponentModel;

namespace QueryFuzzingApi.Models
{
    public class ExecuteQueryAnalysisModel
    {
        [DefaultValue("libming-CVE-2018-8807")]
        public string ProjectName { get; set; }
        [DefaultValue("/home/marius/temp/libming-CVE-2018-8807")]
        public string ProjectPath { get; set; }
        [DefaultValue(Language.c)]
        public Language Language { get; set; }
        [DefaultValue(Tag.none)] 
        public Tag Tag { get; set; }
        [DefaultValue(@"C:\Temp")] 
        public string TargetPath { get; set; } 
    }
}
