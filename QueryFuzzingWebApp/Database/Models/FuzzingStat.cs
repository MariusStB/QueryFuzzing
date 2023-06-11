namespace QueryFuzzingWebApp.Database.Models
{
    public class FuzzingStat
    {
        public int Id { get; set; }
        public int FuzzingInstanceId { get; set; }
        public FuzzingInstance FuzzingInstance { get; set; }
        public DateTime start_time { get; set; }
        public DateTime last_update { get; set; }
        public int fuzzer_pid { get; set; }
        public int cycles_done { get; set; }
        public int execs_done { get; set; }
        public float execs_per_sec { get; set; }
        public int paths_total { get; set; }
        public int paths_favored { get; set; }
        public int paths_found { get; set; }
        public int paths_imported { get; set; }
        public int max_depth { get; set; }
        public int cur_path { get; set; }
        public int pending_favs { get; set; }
        public int pending_total { get; set; }
        public int variable_paths { get; set; }
        public string stability { get; set; }
        public string bitmap_cvg { get; set; }
        public int unique_crashes { get; set; }
        public int unique_hangs { get; set; }
        public DateTime last_path { get; set; }
        public DateTime last_crash { get; set; }
        public DateTime last_hang { get; set; }
    }
}
