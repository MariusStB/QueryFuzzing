using QueryFuzzing.Valgrind.Models;
using System.Text.RegularExpressions;

namespace QueryFuzzing.Valgrind
{
    public class ValgrindService : IValgrindService
    {
        public async Task<List<ValgrindError>> ParseValgrindLog(Stream stream, TimeSpan fuzzingCrashTime)
        {
            StreamReader reader = new StreamReader(stream);
            var valgrindErrorList = new List<ValgrindError>();
            string oldLine = "";
            
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine().Split("==").Last().Trim();

                var regex = new Regex(@"at\s(?<address>0x[0-9A-F]+):\s(?<method>[a-zA-Z0-9-_]+)\s\((?<location>[^\(\)]+)\)");
                var m = regex.Match(line ?? "");
                if (m.Success)
                {
                    int stackCounter = 0;
                    var stacktrace = new List<Trace>();

                    var trace = new Trace
                    {
                        Id = stackCounter,
                        Method = m.Groups["method"].Value,
                    };
                    string location = m.Groups["location"].Value;
                    if (location.StartsWith("in "))
                    {
                        trace.GlobalLib = true;
                        trace.ClassName = location.Substring(3);
                    }
                    else
                    {
                        trace.ClassName = location.Split(":")[0];
                        trace.LineNumber = int.Parse(location.Split(":")[1]);
                    }
                    stacktrace.Add(trace);

                    stackCounter++;
                    while (!reader.EndOfStream)
                    {
                        string traceLine = reader.ReadLine().Split("==").Last().Trim();
                        var regexBy = new Regex(@"by\s(?<address>0x[0-9A-F]+):\s(?<method>[a-zA-Z0-9-_]+)\s\((?<location>[^\(\)]+)\)");

                        var mBy = regexBy.Match(traceLine ?? "");
                        if (mBy.Success)
                        {
                            trace = new Trace
                            {
                                Id = stackCounter,
                                Method = mBy.Groups["method"].Value,
                            };
                            location = mBy.Groups["location"].Value;
                            if (location.StartsWith("in "))
                            {
                                trace.GlobalLib = true;
                                trace.ClassName = location.Substring(3);
                            }
                            else
                            {
                                trace.ClassName = location.Split(":")[0];
                                trace.LineNumber = int.Parse(location.Split(":")[1]);
                            }
                            stacktrace.Add(trace);
                            stackCounter++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    valgrindErrorList.Add(new ValgrindError
                    {
                        ErrorMessage = oldLine.Split("==").Last(),
                        FuzzingCrashTime = fuzzingCrashTime,
                        Stacktrace = stacktrace

                    });

                }
                oldLine = line;
            }

            return valgrindErrorList;
        }
    }
}
