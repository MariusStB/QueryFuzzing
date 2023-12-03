using QueryFuzzing.Models;

namespace QueryFuzzingWebApp.Database.Models
{
    public class CrashedTarget : Target
    {
        public MatchAccuracy MatchAccuracy { get; set; }
        public int CrashId { get; set; }
        public Crash Crash { get; set; }
    }
}
