using System.Collections.Specialized;
using System.Configuration;

namespace PPC.Common
{
    public class PPCConfigurationManager
    {
        private static NameValueCollection _appSettings;
        public static NameValueCollection AppSettings
        {
            get { return _appSettings ?? ConfigurationManager.AppSettings; }
            set { _appSettings = value; }
        }

        private static ConnectionStringSettingsCollection _connectionStrings;
        public static ConnectionStringSettingsCollection ConnectionStrings
        {
            get { return _connectionStrings ?? ConfigurationManager.ConnectionStrings; }
            set { _connectionStrings = value; }
        }

        private static double? _fontSize;
        public static double? FontSize {
            get { return _fontSize ?? SaveConvertToDecimal(AppSettings["FontSize"]); }
            set { _fontSize = value; }
        }

        private static double? SaveConvertToDecimal(string s)
        {
            double d;
            if (double.TryParse(s, out d))
                return d;
            return null;
        }
    }
}
