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

        public static string LogPath => AppSettings["logpath"];
        public static string PlayersPath => AppSettings["PlayersPath"];
        public static string BackupPath => AppSettings["BackupPath"];
        public static string ArticlesPath => AppSettings["ArticlesPath"];
        public static string CashRegisterClosurePath => AppSettings["CashRegisterClosurePath"];
        public static string CashRegisterClosureConfigPath => AppSettings["CashRegisterClosureConfigPath"];
        public static string CashRegisterCountPath => AppSettings["CashRegisterCountPath"];
        public static string CardSellersPath => AppSettings["CardSellersPath"];
        public static bool? UseMongo => SaveConvertToBool(AppSettings["UseMongo"]);

        private static double? _fontSize;
        public static double? FontSize
        {
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

        private static bool? SaveConvertToBool(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return null;
            bool b;
            if (bool.TryParse(s, out b))
                return b;
            return false;
        }
    }
}
