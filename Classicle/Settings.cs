namespace Classicle
{
    public class Settings
    {
        public enum ServerTypes
        {
            None = 0,
            SqlServer = 1,
            MySql = 2,
            SqlLite = 3
        };

        public enum Languages
        {
            None = 0,
            CSharp = 1,
            VbNet = 2
        };

        public ServerTypes ServerType { get; set; }
        public string SqlServerServerName { get; set; }
        public int SqlServerServerPort { get; set; }
        public string SqlServerDatabaseName { get; set; }
        public string SqlServerSchemaName { get; set; }
        public bool SqlServerTrustedConnection { get; set; }
        public string SqlServerUsername { get; set; }
        public string SqlServerPassword { get; set; }
        public string OutputFolder { get; set; }
        public Languages Language { get; set; }
        public string Namespace { get; set; }
        public bool UseDapperExtensions { get; set; }
        public bool UseBackingFields { get; set; }

        public Settings()
        {
            Language = Languages.CSharp;
            OutputFolder = "";
            Namespace = "";
            ServerType = ServerTypes.SqlServer;
            SqlServerServerName = "";
            SqlServerServerPort = 1433;
            SqlServerDatabaseName = "";
            SqlServerSchemaName = "dbo";
            SqlServerTrustedConnection = false;
            SqlServerUsername = "";
            SqlServerPassword = "";
            UseDapperExtensions = true;
            UseBackingFields = false;
        }
    }
}
