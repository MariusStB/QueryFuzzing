using QueryFuzzingWebApp.Database.Models;

namespace QueryFuzzingWebApp.Models
{
    public class QueryFuzzStatusModel
    {
        public Project Project { get; set; }
        public int SelectedInstance { get; set; }

        public FuzzingStatus FuzzingStatus { get; set; }
    }
}
