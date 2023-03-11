using QueryFuzzing.Joern.Models;
using System.ComponentModel;

namespace QueryFuzzingApi.Models
{
    public class ExecuteQueryAnalysisModel
    {
        [DefaultValue("x42-c")]
        public string ProjectName { get; set; }
        [DefaultValue("/home/marius/temp/x42/c")]
        public string ProjectPath { get; set; }
        [DefaultValue(Language.c)]
        public Language Language { get; set; }
        [DefaultValue(Tag.none)] 
        public Tag Tag { get; set; }
        [DefaultValue(@"C:\Temp")] 
        public string TargetPath { get; set; } 
    }
}
