using QueryFuzzingWebApp.Database.Models;

namespace QueryFuzzingWebApp.Models
{
    public class FuzzingResult
    {
        public FuzzingStat Status { get; set; }

        public List<Crash> Crashes { get; set; }  
        public List<InstanceTarget> FalsePositives { get; set; }
    }
}
