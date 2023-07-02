using QueryFuzzing.Joern.Models;
using System.Text.Json;

namespace QueryFuzzing.Joern
{
    public static class QueryListParser
    {
        public static List<QueryCallMatch> ParseCallList(string input)
        {
            try
            {
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
