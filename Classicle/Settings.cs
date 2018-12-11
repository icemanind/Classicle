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
        #region "SQL Server Settings"
        public string SqlServerServerName { get; set; }
        public int SqlServerServerPort { get; set; }
        public string SqlServerDatabaseName { get; set; }
        public string SqlServerSchemaName { get; set; }
        public bool SqlServerTrustedConnection { get; set; }
        public string SqlServerUsername { get; set; }
        public string SqlServerPassword { get; set; }
        #endregion
        #region "Sqlite Settings"
        public string SqliteFileName { get; set; }
        public string SqlitePassword { get; set; }
        #endregion
        #region "MySql Settings"
        public string MySqlServerName { get; set; }
        public int MySqlServerPort { get; set; }
        public string MySqlDatabaseName { get; set; }
        public string MySqlUsername { get; set; }
        public string MySqlPassword { get; set; }
        #endregion
        #region "Language Settings"
        public string OutputFolder { get; set; }
        public Languages Language { get; set; }
        public string Namespace { get; set; }
        public bool UseDapperExtensions { get; set; }
        public bool UseBackingFields { get; set; }
        #endregion

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
            SqliteFileName = "";
            SqlitePassword = "";
            MySqlServerName = "";
            MySqlServerPort = 3306;
            MySqlDatabaseName = "";
            MySqlUsername = "";
            MySqlPassword = "";
            UseDapperExtensions = true;
            UseBackingFields = false;
        }
    }
}
