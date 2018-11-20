using System.Collections.Generic;

namespace Classicle
{
    internal interface IDatabase
    {
        string ConnectionString { get; }
        string DefaultSchema { get; set; }
        string DatabaseName { get; set; }
        string ServerName { get; set; }
        string DefaultNamespace { get; set; }
        int ServerPort { get; set; }
        Settings.Languages Language { get; set; }
        bool UseDapperExtensions { get; set; }
        bool UseBackingFields { get; set; }
        bool TrustedConnection { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        List<Field> MapFields(string tableName);
        List<ClassicleObject> GetTablesAndViews();
        void CreateLayers(List<ObjectViewModel> objects, string outputFolder);
    }
}
