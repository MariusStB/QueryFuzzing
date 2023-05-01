﻿
using QueryFuzzing.Joern.Models;
using System.Text;

namespace QueryFuzzing.TargetFuzzing
{
    public class TargetCreator
    {
        public static byte[] CreateTargetFile(List<QueryCallMatch> matches, string path, bool excludeTest=true)
        {
            var sb = new StringBuilder();
            byte[] buffer = new byte[10*1024];
            foreach (var match in matches.GroupBy(m => new {m.LineNumber, m.Filename}).Select(m => m.First()).OrderBy(o=> o.Filename))
            {
                if(excludeTest)
                {
                    if (match.Filename.Contains("test")) { 
                        continue; 
                    }
                }
                sb.AppendLine($"{match.Filename}:{match.LineNumber}");
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

                    return ms.ToArray();
                }
            }

            //using (var sw = new StreamWriter(path))
            //{
            //    sw.Write(sb.ToString());
            //    return sw.ToArray();
            //}
        }
    }
}
