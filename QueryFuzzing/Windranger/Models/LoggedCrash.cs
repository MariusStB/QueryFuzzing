
namespace QueryFuzzing.Windranger.Models
{
    public class LoggedCrash
    {
        public int Id { get; set; }
        public int Days { get; set; }   
        public int Hours { get; set; } 
        public int Minutes { get; set; }
        public int Seconds { get; set; }
        public string Execution { get; set; }
    }
}
