using QueryFuzzing.Joern.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace QueryFuzzing.Joern
{
    public static class QueryListParser
    {
        public static List<QueryMethodMatch> ParseMethodList(string input)
        {
            var regex = new Regex(@"\((\s)*id\s=\s(?<id>[A-Z0-9]+),(\s)*astParentFullName\s=\s""(?<astParentFullName>[^""]*)"",(\s)*astParentType\s=\s""(?<astParentType>[^""]*)"",(\s)*code\s=\s""(?<code>[^""]*)"",(\s)*columnNumber\s=\sSome\(value\s=\s(?<columnNumber>[0-9]+)\),(\s)*columnNumberEnd\s=\sSome\(value\s=\s(?<columnNumberEnd>[0-9]+)\),(\s)*filename\s=\s""(?<filename>[^""]*)"",(\s)*fullName\s=\s""(?<fullName>[^""]*)"",(\s)*hash\s=\s(?<hash>[^""]*),(\s)*isExternal\s=\s(?<isExternal>[^""]*),(\s)*lineNumber\s=\sSome\(value\s=\s(?<lineNumber>[0-9]+)\),(\s)*lineNumberEnd\s=\sSome\(value\s=\s(?<lineNumberEnd>[0-9]+)\),(\s)*name\s=\s""(?<name>[^""]*)"",(\s)*order\s=\s(?<order>[^""]*),(\s)*signature\s=\s""(?<signature>[^""]*)""(\s)*\)"
                , RegexOptions.IgnorePatternWhitespace);

            var matchList = new List<QueryMethodMatch>();

            foreach (Match m in regex.Matches(input))
            {
                matchList.Add(new QueryMethodMatch
                {
                    Id = m.Groups["id"].Value,
                    AstParentFullName = m.Groups["astParentFullName"].Value,
                    AstParentType = m.Groups["astParentType"].Value,
                    Code = m.Groups["code"].Value,
                    ColumnNumber = int.Parse(m.Groups["columnNumber"].Value),
                    ColumnNumberEnd = int.Parse(m.Groups["columnNumberEnd"].Value),
                    Filename = m.Groups["filename"].Value,
                    FullName = m.Groups["fullName"].Value,
                    Hash = m.Groups["hash"].Value,
                    IsExternal = m.Groups["isExternal"].Value,
                    LineNumber = int.Parse(m.Groups["lineNumber"].Value),
                    LineNumberEnd = int.Parse(m.Groups["lineNumberEnd"].Value),
                    Name = m.Groups["name"].Value,
                    Order = m.Groups["order"].Value,
                    Signature = m.Groups["signature"].Value
                });               
            }

            return matchList;
        }

        public static List<QueryCallMatch> ParseCallList(string input)
        {
            try
            {
                //TODO: Vernüftigen Json-Parser einbauen
                Console.WriteLine(input);
                int i = input.IndexOf("String = ");
                string json = input.Substring(i+10);
                json = json.Substring(0, json.Length - 2);
                json = json.Replace("\\", "");
                Console.WriteLine(json);
                var matchList = JsonSerializer.Deserialize<List<QueryCallMatch>>(json);
                return matchList;
            }catch(Exception ex)
            {
                return new List<QueryCallMatch>();
            }

            
        }      

    }
}
