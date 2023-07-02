namespace QueryFuzzingWebApp.Database.Models
{
    public class Target
    {
        public int Id { get; set; }
        public string File { get; set; }
        public string Path { get; set; }
        public int Line { get; set; }
        public string Methodname { get; set; }

    }
}
