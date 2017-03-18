using System;

namespace PPC.Helpers
{
    public static class StringExtensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public static string AppendIfNotEmpty(this string source, string toAdd)
        {
            if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(toAdd))
                return source;
            return source + toAdd;
        }
    }
}
