using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Aerolt.Enums;
using UnityEngine;

namespace Aerolt.Helpers
{
    public static class Tools
    {
        public static T[] FindMatches<T>(T[] toMatch, Func<T, string> toString, string filter)
        {
            Regex filterRegex = new Regex(filter, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            List<T> matches = new List<T>();
            foreach (T obj in toMatch)
            {
                if (filterRegex.IsMatch(toString(obj))) matches.Add(obj);
            }
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