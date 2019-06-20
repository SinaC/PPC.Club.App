using PPC.Common;
using System;
using System.IO;

namespace PPC.DataAccess.MongoDB
{
    // TODO: add IMongoSettings, remove static
    public static class MongoSettings
    {
        public static string ConnectionString
        {
            get
            {
                try
                {
                    return File.ReadAllText(PPCConfigurationManager.MongoSettingsPath);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
    }
}
