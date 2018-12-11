using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace Classicle.Implementations.Database
{
    internal class Sqlite : IDatabase
    {
        public string DefaultSchema { get; set; }
        public string DatabaseName { get; set; }
        public string ServerName { get; set; }
        public Classicle.Settings.Languages Language { get; set; }
        public bool TrustedConnection { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DefaultNamespace { get; set; }
        public int ServerPort { get; set; }
        public bool UseDapperExtensions { get; set; }
        public bool UseBackingFields { get; set; }

        public string ConnectionString
        {
            get
            {
                var builder = new SQLiteConnectionStringBuilder
                {
                    DataSource = DatabaseName,
                    JournalMode = SQLiteJournalModeEnum.Memory,
                    Password = string.IsNullOrEmpty(Password) ? null : Password.Trim()
                };

                return builder.ConnectionString;
            }
        }

        public List<ClassicleObject> GetTablesAndViews()
        {
            var classicleObjects = new List<ClassicleObject>();

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                using (DataTable tables = connection.GetSchema("Tables"))
                {
                    foreach (DataRow row in tables.Rows.Cast<DataRow>().Where(row => ((string)row["TABLE_TYPE"]).ToLower() == "table"))
                    {
                        var obj = new ClassicleObject
                        {
                            IsView = false,
                            ObjectName = (string)row["TABLE_NAME"]
                        };

                        classicleObjects.Add(obj);
                    }
                }

                using (DataTable views = connection.GetSchema("Views"))
                {
                    foreach (DataRow row in views.Rows)
                    {
                        var obj = new ClassicleObject
                        {
                            IsView = true,
                            ObjectName = (string)row["TABLE_NAME"]
                        };

                        classicleObjects.Add(obj);
                    }
                }
            }

            return classicleObjects;
        }

        public List<Field> MapFields(string tableName)
        {
            var fields = new List<Field>();

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                using (DataTable columns = connection.GetSchema("Columns"))
                {
                    foreach (DataRow row in columns.Rows)
                    {
                        if (((string)row["TABLE_NAME"]).ToLower() != tableName.ToLower())
                            continue;

                        var field = new Field
                        {
                            FieldName = (string) row["COLUMN_NAME"],
                            IsPrimaryKey = (bool) row["PRIMARY_KEY"],
                            IsIdentity = (bool) row["AUTOINCREMENT"]
                        };
                        field.CsDataType = GetCsDataType(((string)row["DATA_TYPE"]).Trim(), field.IsIdentity);
                        field.VbDataType = GetVbDataType(((string)row["DATA_TYPE"]).Trim(), field.IsIdentity);
                        field.TextBased = IsTextBased(GetCsDataType(((string)row["DATA_TYPE"]).Trim(), field.IsIdentity));
                        field.SafeCsFieldName = Utility.GetSafeCsName(Utility.GetCsFieldName(field.FieldName));
                        field.SafeCsPropertyName = Utility.GetSafeCsName(Utility.GetCsPropertyName(field.FieldName));
                        field.SafeVbFieldName = Utility.GetSafeVbName(Utility.GetVbFieldName(field.FieldName));
                        field.SafeVbPropertyName = Utility.GetSafeVbName(Utility.GetVbPropertyName(field.FieldName));
                        field.SqlDbType = GetSqlDbTypeFromSqlType(((string)row["DATA_TYPE"]).Trim());
                        field.IntrinsicSqlDataType = ((string)row["DATA_TYPE"]).Trim();
                        field.IsValueType = Utility.IsValueType(field.CsDataType);
                        // A column declared INTEGER PRIMARY KEY will autoincrement: http://www.sqlite.org/faq.html#q1
                        if (field.IntrinsicSqlDataType.ToLower() == "integer" && field.IsPrimaryKey)
                        {
                            field.IsIdentity = true;
                        }
                        fields.Add(field);
                    }
                }
            }

            return fields;
        }

        public void CreateLayers(List<ObjectViewModel> objects, string outputFolder)
        {
            ILanguage language;

            switch (Language)
            {
                case Classicle.Settings.Languages.CSharp:
                    language = new Language.CSharp();
                    break;
                case Classicle.Settings.Languages.VbNet:
                    language = new Language.VbNet();
                    break;
                default:
                    language = new Language.CSharp();
                    break;
            }

            string classicleFile = Path.Combine(outputFolder, $"Classicle{language.FileNameExtension}");
            File.WriteAllText(classicleFile, language.GetClassicalFile(DefaultNamespace, ConnectionString));

            foreach (ObjectViewModel obj in objects)
            {
                string fileName = Utility.GetVbPropertyName(obj.ObjectName);
                string file = Path.Combine(outputFolder, fileName) + language.FileNameExtension;
                var sb = new StringBuilder();

                List<Field> fields = MapFields(obj.ObjectName);

                sb.AppendLine(language.GetImportsCode(UseDapperExtensions));
                sb.AppendLine();
                sb.AppendLine(language.GetNamespaceCode(DefaultNamespace));
                sb.AppendLine(language.GetOpeningNamespaceMarker());
                if (UseDapperExtensions)
                {
                    sb.AppendLine($"{Utility.GetIndentSpaces(4)}{language.GetAttributeOpeningMarker()}Table(\"{obj.ObjectName}\"){language.GetAttributeClosingMarker()}");
                }
                sb.AppendLine(Utility.GetIndentSpaces(4) + language.GetPublicClassCode(obj.ObjectName));
                sb.AppendLine(Utility.GetIndentSpaces(4) + language.GetOpeningClassMarker());

                if (UseBackingFields)
                {
                    foreach (Field field in fields)
                    {
                        if (!string.IsNullOrWhiteSpace(field.Description))
                        {
                            sb.AppendLine($"{Utility.GetIndentSpaces(8)}{language.GetSingleLineCommentMarker()} { language.GetSafeFieldName(field) } - {field.Description}");
                        }

                        sb.AppendLine($"{Utility.GetIndentSpaces(8)}{language.GetPrivateBackingFieldCode(field)}");
                    }

                    sb.AppendLine();
                }

                foreach (Field field in fields)
                {
                    if (!string.IsNullOrWhiteSpace(field.Description))
                    {
                        sb.AppendLine($"{Utility.GetIndentSpaces(8)}{language.GetSingleLineCommentMarker()} { language.GetSafePropertyName(field) } - {field.Description}");
                    }

                    if (field.IsPrimaryKey && field.IsIdentity && UseDapperExtensions)
                    {
                        sb.AppendLine($"{Utility.GetIndentSpaces(8)}{language.GetAttributeOpeningMarker()}Key{language.GetAttributeClosingMarker()}");
                    }
                    else if (field.IsPrimaryKey && UseDapperExtensions)
                    {
                        sb.AppendLine($"{Utility.GetIndentSpaces(8)}{language.GetAttributeOpeningMarker()}ExplicitKey{language.GetAttributeClosingMarker()}");
                    }

                    if (field.IsComputedField && UseDapperExtensions)
                    {
                        sb.AppendLine($"{Utility.GetIndentSpaces(8)}{language.GetAttributeOpeningMarker()}Computed{language.GetAttributeClosingMarker()}");
                        sb.AppendLine($"{Utility.GetIndentSpaces(8)}{language.GetAttributeOpeningMarker()}Write(false){language.GetAttributeClosingMarker()}");
                    }

                    sb.AppendLine(language.GetPublicPropertyCode(field, UseBackingFields, 8));
                }

                sb.AppendLine($"{Utility.GetIndentSpaces(4)}{language.GetClosingClassMarker()}");
                sb.AppendLine($"{language.GetClosingNamespaceMarker()}");

                File.WriteAllText(file, sb.ToString());
            }
        }

        private bool IsTextBased(string csType)
        {
            return csType.ToLower() == "string";
        }

        private string GetSqlDbTypeFromSqlType(string sqlType)
        {
            sqlType = sqlType.Trim().ToLower();

            switch (sqlType)
            {
                case "tinyint":
                    return "Byte";
                case "integer":
                case "bigint":
                case "int64":
                case "largeint":
                case "word":
                    return "Int64";
                case "int":
                    return "Int32";
                case "nvarchar":
                case "nvarchar2":
                case "varchar":
                case "varchar2":
                case "text":
                case "char":
                case "nchar":
                case "ntext":
                case "datetext":
                case "clob":
                case "memo":
                case "xml":
                case "string":
                    return "String";
                case "float":
                case "double precision":
                case "double":
                case "smallmoney":
                case "dec":
                    return "Double";
                case "real":
                    return "Double";
                case "decimal":
                case "numeric":
                case "money":
                case "number":
                case "currency":
                    return "Currency";
                case "boolean":
                case "bool":
                case "bit":
                    return "Boolean";
                case "time":
                case "date":
                case "datetime":
                case "timestamp":
                    return "DateTime";
                case "blob":
                case "binary":
                case "blob_text":
                case "image":
                case "graphic":
                case "raw":
                case "photo":
                case "picture":
                case "varbinary":
                    return "Binary";
                case "smallint":
                    return "Int16";
                case "guid":
                case "uniqueidentifier":
                    return "Guid";
            }

            return sqlType;
        }

        private string GetVbDataType(string sqliteDataType, bool isIdentity)
        {
            if (isIdentity)
                return "Long";

            string sqlLiteDataTypeLower = sqliteDataType.ToLower();

            if (sqlLiteDataTypeLower == "tinyint")
                return "Byte";
            if (sqlLiteDataTypeLower == "integer" || sqlLiteDataTypeLower == "bigint" || sqlLiteDataTypeLower == "int64" || sqlLiteDataTypeLower == "largeint" ||
                sqlLiteDataTypeLower == "word")
                return "Long";
            if (sqlLiteDataTypeLower == "int" || sqlLiteDataTypeLower == "tinyint")
                return "Integer";
            if (sqlLiteDataTypeLower == "nvarchar" || sqlLiteDataTypeLower == "varchar" || sqlLiteDataTypeLower == "text" || sqlLiteDataTypeLower == "datetext" ||
                sqlLiteDataTypeLower == "char" || sqlLiteDataTypeLower == "nchar" || sqlLiteDataTypeLower == "clob" || sqlLiteDataTypeLower == "xml" ||
                sqlLiteDataTypeLower == "memo" || sqlLiteDataTypeLower == "ntext" || sqlLiteDataTypeLower == "nvarchar2" || sqlLiteDataTypeLower == "varchar2" ||
                sqlLiteDataTypeLower == "string")
                return "String";
            if (sqlLiteDataTypeLower == "float" || sqlLiteDataTypeLower == "double precision" || sqlLiteDataTypeLower == "double" || sqlLiteDataTypeLower == "dec" ||
                sqlLiteDataTypeLower == "smallmoney")
                return "Double";
            if (sqlLiteDataTypeLower == "real")
                return "Double";
            if (sqlLiteDataTypeLower == "numeric" || sqlLiteDataTypeLower == "currency" || sqlLiteDataTypeLower == "decimal" || sqlLiteDataTypeLower == "money" ||
                sqlLiteDataTypeLower == "number")
                return "Decimal";
            if (sqlLiteDataTypeLower == "boolean" || sqlLiteDataTypeLower == "bit" || sqlLiteDataTypeLower == "bool")
                return "Boolean";
            if (sqlLiteDataTypeLower == "time" || sqlLiteDataTypeLower == "date" || sqlLiteDataTypeLower == "timestamp" || sqlLiteDataTypeLower == "datetime")
                return "DateTime";
            if (sqlLiteDataTypeLower == "blob" || sqlLiteDataTypeLower == "blob_text" || sqlLiteDataTypeLower == "binary" ||
                sqlLiteDataTypeLower == "graphic" || sqlLiteDataTypeLower == "image" || sqlLiteDataTypeLower == "picture" || sqlLiteDataTypeLower == "photo" ||
                sqlLiteDataTypeLower == "raw" || sqlLiteDataTypeLower == "varbinary")
                return "Byte()";
            if (sqlLiteDataTypeLower == "smallint")
                return "Short";
            if (sqlLiteDataTypeLower == "guid" || sqlLiteDataTypeLower == "uniqueidentifier")
                return "Guid";

            return sqliteDataType.ToUpper();
        }

        private string GetCsDataType(string sqliteDataType, bool isIdentity)
        {
            if (isIdentity)
                return "long";

            string sqlLiteDataTypeLower = sqliteDataType.ToLower();

            if (sqlLiteDataTypeLower == "tinyint")
                return "byte";
            if (sqlLiteDataTypeLower == "integer" || sqlLiteDataTypeLower == "bigint" || sqlLiteDataTypeLower == "int64" || sqlLiteDataTypeLower == "largeint" ||
                sqlLiteDataTypeLower == "word")
                return "long";
            if (sqlLiteDataTypeLower == "int" || sqlLiteDataTypeLower == "tinyint")
                return "int";
            if (sqlLiteDataTypeLower == "nvarchar" || sqlLiteDataTypeLower == "varchar" || sqlLiteDataTypeLower == "text" || sqlLiteDataTypeLower == "datetext" ||
                sqlLiteDataTypeLower == "char" || sqlLiteDataTypeLower == "nchar" || sqlLiteDataTypeLower == "clob" || sqlLiteDataTypeLower == "xml" ||
                sqlLiteDataTypeLower == "memo" || sqlLiteDataTypeLower == "ntext" || sqlLiteDataTypeLower == "nvarchar2" || sqlLiteDataTypeLower == "varchar2" ||
                sqlLiteDataTypeLower == "string")
                return "string";
            if (sqlLiteDataTypeLower == "float" || sqlLiteDataTypeLower == "double precision" || sqlLiteDataTypeLower == "double" || sqlLiteDataTypeLower == "dec" ||
                sqlLiteDataTypeLower == "smallmoney")
                return "double";
            if (sqlLiteDataTypeLower == "real")
                return "double";
            if (sqlLiteDataTypeLower == "numeric" || sqlLiteDataTypeLower == "currency" || sqlLiteDataTypeLower == "decimal" || sqlLiteDataTypeLower == "money" ||
                sqlLiteDataTypeLower == "number")
                return "decimal";
            if (sqlLiteDataTypeLower == "boolean" || sqlLiteDataTypeLower == "bit" || sqlLiteDataTypeLower == "bool")
                return "bool";
            if (sqlLiteDataTypeLower == "time" || sqlLiteDataTypeLower == "date" || sqlLiteDataTypeLower == "timestamp" || sqlLiteDataTypeLower == "datetime" ||
                sqlLiteDataTypeLower == "datetext")
                return "DateTime";
            if (sqlLiteDataTypeLower == "blob" || sqlLiteDataTypeLower == "blob_text" || sqlLiteDataTypeLower == "binary" ||
                sqlLiteDataTypeLower == "graphic" || sqlLiteDataTypeLower == "image" || sqlLiteDataTypeLower == "picture" || sqlLiteDataTypeLower == "photo" ||
                sqlLiteDataTypeLower == "raw" || sqlLiteDataTypeLower == "varbinary")
                return "byte[]";
            if (sqlLiteDataTypeLower == "smallint")
                return "short";
            if (sqlLiteDataTypeLower == "guid" || sqlLiteDataTypeLower == "uniqueidentifier")
                return "Guid";

            return sqliteDataType.ToUpper();
        }
    }
}
