using System;
using System.Collections.Generic;

namespace Classicle.Implementations.Database
{
    internal class None : IDatabase
    {
        public string ConnectionString { get; }
        public string DefaultSchema { get; set; }
        public string DatabaseName { get; set; }
        public string ServerName { get; set; }
        public string DefaultNamespace { get; set; }
        public int ServerPort { get; set; }
        public Classicle.Settings.Languages Language { get; set; }
        public bool UseDapperExtensions { get; set; }
        public bool UseBackingFields { get; set; }
        public bool TrustedConnection { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public List<Field> MapFields(string tableName)
        {
            throw new NotImplementedException();
        }

        public List<ClassicleObject> GetTablesAndViews()
        {
            throw new NotImplementedException();
        }

        public void CreateLayers(List<ObjectViewModel> objects, string outputFolder)
        {
            throw new NotImplementedException();
        }
    }
}
