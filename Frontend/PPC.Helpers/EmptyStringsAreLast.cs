using System.Collections.Generic;

namespace PPC.Helpers
{
    public class EmptyStringsAreLast : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (string.IsNullOrEmpty(y) && !string.IsNullOrEmpty(x))
                return -1;
            if (!string.IsNullOrEmpty(y) && string.IsNullOrEmpty(x))
                return 1;
            return string.CompareOrdinal(x, y);
        }
    }
}
