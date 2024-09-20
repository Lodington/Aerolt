using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Aerolt.Enums;

namespace Aerolt.Helpers
{
    public static class Tools
    {
        public static string SendCount()
        {  
            const string uri = "https://links.lodington.dev/aerolt";
            var client = new WebClient();
            
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            var data = client.OpenRead(uri);
            var reader = new StreamReader(data!);
            var s = reader.ReadToEnd();
            data!.Close();
            reader.Close();

            return s;
    }
        
        public static T[] FindMatches<T>(T[] toMatch, Func<T, string> toString, string filter)
        {
            var filterRegex = new Regex(filter, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matches = new List<T>();
            foreach (var obj in toMatch)
                if (filterRegex.IsMatch(toString(obj)))
                    matches.Add(obj);
            return matches.ToArray();
        }

        public static void Log(LogLevel level, object s)
        {
            switch (level)
            {
                case LogLevel.Warning:
                    Load.Log.LogWarning(s.ToString());
                    break;
                case LogLevel.Error:
                    Load.Log.LogError(s.ToString());
                    break;
                case LogLevel.Information:
                    Load.Log.LogMessage(s.ToString());
                    break;
            }
        }
    }
}