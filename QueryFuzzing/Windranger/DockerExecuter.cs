using System.Diagnostics;
using System.Text;

namespace QueryFuzzing.Windranger
{
    public class DockerExecuter
    {
        private static List<string> s_output = new List<string>();
        public static List<string> ExecDockerCommand(string command, int timeout = 0)
        {
            s_output.Clear();
            var processInfo = new ProcessStartInfo("docker", command);

            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;
            int exitCode;
            using (var process = new Process())
            {
                process.StartInfo = processInfo;

                process.OutputDataReceived += new DataReceivedEventHandler(Output);
                process.ErrorDataReceived += new DataReceivedEventHandler(Log);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                if(timeout!=0)
                {
                    process.WaitForExit(timeout);
                }else
                {
                    process.WaitForExit();
                }
                if (!process.HasExited)
                {
                    process.Kill();
                }

                exitCode = process.ExitCode;
                process.Close();
            }
           
            return s_output;
            
        }

        private static void Output(object sender, DataReceivedEventArgs e)
        {
            if(e.Data != null)
            {
                s_output.Add(e.Data);
            }
        }

        private static void Log(object sender, DataReceivedEventArgs e)
        {

            Console.Write($"{e.Data}{Environment.NewLine}");

        }


    }
}
