using QueryFuzzingWebApp.Database.Models;
using System.Text;

namespace QueryFuzzingWebApp.Services
{
    public class TargetCreator
    {
        public static void CreateTargetFile(List<InstanceTarget> targets, string path)
        {
            var sb = new StringBuilder();
            byte[] buffer = new byte[10 * 1024];
            foreach (var target in targets.GroupBy(m => new { m.Line, m.File }).Select(m => m.First()).OrderBy(o => o.File))
            {
                
                sb.AppendLine($"{target.File}:{target.Line}");
            }

            using (var ms = new MemoryStream())
            {
                using (var writer = new StreamWriter(ms))
                {
                    writer.Write(sb.ToString());
                    writer.Flush();
                    ms.Position = 0;
                    FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write);
                    ms.WriteTo(file);
                    file.Close();

                    return;
                }
            }            
        }
    }
}
