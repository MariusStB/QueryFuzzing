namespace QueryFuzzingWebApp.Database.Models
{
    public class ProjectTarget:Target
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; }
    }
}
