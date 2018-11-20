namespace Classicle.Implementations.Settings
{
    public class StaticSettings : ISettings
    {
        public Classicle.Settings LoadSettings()
        {
            // Static settings just for testing
            var settings = new Classicle.Settings
            {
                Language = Classicle.Settings.Languages.CSharp,
                OutputFolder = @"C:\q\Classicle",
                ServerType = Classicle.Settings.ServerTypes.SqlServer,
                SqlServerServerName = "132.148.80.197",
                SqlServerServerPort = 1433,
                SqlServerDatabaseName = "OnQ",
                SqlServerSchemaName = "dbo",
                SqlServerTrustedConnection = false,
                SqlServerUsername = "sa",
                SqlServerPassword = "sLa#zip7",
                Namespace = "OnQ",
                UseDapperExtensions = true,
                UseBackingFields = false
            };

            return settings;
        }

        public void SaveSettings(Classicle.Settings settings)
        {
            // This is for testing, so we are not going to do anything in this method.
            // We will display a message box though so that we know this method is getting called.
            // MessageBox.Show("Settings Saved!");
        }
    }
}
