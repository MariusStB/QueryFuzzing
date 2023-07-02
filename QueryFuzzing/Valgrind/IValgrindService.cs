using QueryFuzzing.Valgrind.Models;

namespace QueryFuzzing.Valgrind
{
    public interface IValgrindService
    {
        Task<List<ValgrindError>> ParseValgrindLog(Stream stream, TimeSpan fuzzingCrashTime);
    }
}
