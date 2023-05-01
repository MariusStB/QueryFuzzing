namespace QueryFuzzingWebApp.Database.Models
{
    public class Executable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public int FuzzingInstanceId { get; set; }
        public FuzzingInstance FuzzingInstance { get; set; }
    }
}
