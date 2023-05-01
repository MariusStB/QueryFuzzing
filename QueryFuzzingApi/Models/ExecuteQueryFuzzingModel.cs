namespace QueryFuzzingApi.Models
{
    public class ExecuteQueryFuzzingModel
    {
        public string RepositoryZipPath { get; set; }
        public IFormFile? FuzzingInput { get; set; }
    }
}
