using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Aerolt.Enums;

namespace Aerolt.Helpers
{
    public static class Tools
    {
        private const string SendCountUri = "https://links.lodington.dev/aerolt";

        private static readonly HttpClient HttpClient = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            client.DefaultRequestHeaders.TryAddWithoutValidation("user-agent",
                "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
            return client;
        }

        public static void SendCount()
        {
            _ = SendCountAsync();
        }

        private static async Task SendCountAsync()
        {
            try
            {
                var response = await HttpClient.GetStringAsync(SendCountUri).ConfigureAwait(false);
                Log(LogLevel.Information, $"SendCount: {response}");
            }
            catch (Exception e)
            {
                Log(LogLevel.Warning, $"SendCount failed: {e.Message}");
            }
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