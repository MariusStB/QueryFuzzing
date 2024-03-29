﻿using QueryFuzzing.Joern.Models;


namespace QueryFuzzing.Joern
{
    public interface IJoernService
    {
        Task<bool> ImportProject(string projectName, string projectPath);

        Task<List<string>> SendQuery(string query);

        Task<List<Query>> GetQueryDbItems(Language language, Tag tag);
    }
}
