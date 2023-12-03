
namespace QueryFuzzingWebApp.Database.Models
{
    public class Crash
    {
        public int Id { get; set; }
        public TimeSpan CrashTime { get; set; }
        public string Description { get; set; }
        public ICollection<Stacktrace> Stacktrace { get; set; } = new List<Stacktrace>();
        public ICollection<CrashedTarget> CrashedTargets { get; set; } = new List<CrashedTarget>();
        public int FuzzingInstanceId { get; set; }
        public FuzzingInstance FuzzingInstance { get; set; }
    }
}
