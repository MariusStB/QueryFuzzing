using QueryFuzzing.Models;
using QueryFuzzingWebApp.Database.Models;

namespace QueryFuzzingWebApp.Models
{
    public class QueryFuzzResultModel
    {
        public Project Project { get; set; }
        public int SelectedInstance { get; set; }

        public FuzzingResult FuzzingResult { get; set; }

    }
}
