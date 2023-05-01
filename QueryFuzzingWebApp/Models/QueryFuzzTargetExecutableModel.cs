using QueryFuzzingWebApp.Database.Models;

namespace QueryFuzzingWebApp.Models
{
    public class QueryFuzzTargetExecutableModel
    {
        public Project Project { get; set; }
        public int SelectedInstance { get; set; }

        public int SelectedExecutable { get; set; }
        public List<int> Targets { get; set; }

    }
}
