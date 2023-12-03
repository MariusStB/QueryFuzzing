using QueryFuzzingWebApp.Database.Models;

namespace QueryFuzzing.Models
{
    public class FuzzingResult
    {
        public FuzzingStat Status { get; set; }

        public List<Crash> Crashes { get; set; }
        public List<InstanceTarget> FalsePositives { get; set; }
    }
}
