using QueryFuzzing.Joern.Models;

namespace QueryFuzzingWebApp.Database.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public Language Language { get; set; }
        public Tag Tag { get; set; }
        public DateTime CreatedDate { get; set; }
        public TimeSpan QueryTime { get; set; }
        public ICollection<ProjectTarget> Targets { get; set; } = new List<ProjectTarget>();
        public ICollection<FuzzingInstance> FuzzingInstance { get; set; } = new List<FuzzingInstance>();
    }
}
