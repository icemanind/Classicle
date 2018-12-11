using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using System.IO;
using System.Linq;
using System.Text;


namespace Classicle.Implementations.Database
{
    internal class MySql : IDatabase
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
                var builder = new MySqlConnectionStringBuilder
                {
                    UserID = Username,
                    Password = Password,
                    Database = DatabaseName,
                    Server = ServerName,
                    ConvertZeroDateTime = true,
                    Port = (uint)ServerPort
                };

                return builder.ConnectionString;
            }
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

        public List<ClassicleObject> GetTablesAndViews()
        {
            var classicleObjects = new List<ClassicleObject>();

            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                using (DataTable tables = connection.GetSchema("Tables"))
                {
                    foreach (DataRow row in tables.Rows.Cast<DataRow>())
                    {
                        if (((string)row["TABLE_SCHEMA"]) != DatabaseName)
                            continue;
                        var obj = new ClassicleObject();
                        obj.IsView = false;
                        obj.ObjectName = (string)row["TABLE_NAME"];

                        classicleObjects.Add(obj);
                    }
                }

                using (DataTable views = connection.GetSchema("Views"))
                {
                    foreach (DataRow row in views.Rows)
                    {
                        if (((string)row["TABLE_SCHEMA"]) != DatabaseName)
                            continue;
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

            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                using (DataTable columns = connection.GetSchema("Columns"))
                {
                    foreach (DataRow row in columns.Rows)
                    {
                        if (((string)row["TABLE_NAME"]).ToLower() != tableName.ToLower())
                            continue;

                        var field = new Field();
                        field.FieldName = (string)row["COLUMN_NAME"];
                        field.IsPrimaryKey = ((string)row["COLUMN_KEY"]).ToLower().Equals("pri");
                        field.IsUnsigned = ((string)row["COLUMN_TYPE"]).ToLower().EndsWith("unsigned");
                        field.IntrinsicSqlDataType = (string)row["COLUMN_TYPE"];
                        if (((string)row["COLUMN_TYPE"]).ToLower() == "tinyint(1)")
                        {
                            field.SqlDbType = "Bit";
                            field.CsDataType = "bool";
                            field.VbDataType = "Boolean";
                        }
                        else
                        {
                            field.SqlDbType = GetSqlDbTypeFromSqlType(((string)row["DATA_TYPE"]).Trim(), field.IsUnsigned);
                            field.CsDataType = GetCsDataType(((string)row["DATA_TYPE"]).Trim(), field.IsIdentity, field.IsUnsigned);
                            field.VbDataType = GetVbDataType(((string)row["DATA_TYPE"]).Trim(), field.IsIdentity, field.IsUnsigned);
                        }
                        field.IsIdentity = ((string)row["EXTRA"]).ToLower().Equals("auto_increment");

                        field.TextBased = IsTextBased(GetCsDataType(((string)row["DATA_TYPE"]).Trim(), field.IsIdentity, field.IsUnsigned));
                        field.SafeCsFieldName = Utility.GetSafeCsName(Utility.GetCsFieldName(field.FieldName));
                        field.SafeCsPropertyName = Utility.GetSafeCsName(Utility.GetCsPropertyName(field.FieldName));
                        field.SafeVbFieldName = Utility.GetSafeVbName(Utility.GetVbFieldName(field.FieldName));
                        field.SafeVbPropertyName = Utility.GetSafeVbName(Utility.GetVbPropertyName(field.FieldName));
                        field.Description = (string)row["COLUMN_COMMENT"];
                        field.IsValueType = Utility.IsValueType(field.CsDataType);
                        field.CanBeNull = ((string)row["IS_NULLABLE"]).ToLower() == "yes";
                        fields.Add(field);
                    }
                }
            }

            return fields;
        }

        private bool IsTextBased(string csType)
        {
            return csType.ToLower() == "string";
        }

        private string GetSqlDbTypeFromSqlType(string sqlType, bool isUnsigned)
        {
            sqlType = sqlType.EndsWith("identity") ? sqlType.Substring(0, sqlType.Length - 8) : sqlType;
            sqlType = sqlType.Trim().ToLower();

            switch (sqlType)
            {
                case "blob":
                    return "Blob";
                case "bigint":
                    return "Int64";
                case "binary":
                    return "Binary";
                case "bit":
                    return "Bit";
                case "char":
                    return "VarChar";
                case "date":
                    return "Date";
                case "datetime":
                    return "DateTime";
                case "datetime2":
                    return "DateTime2";
                case "datetimeoffset":
                    return "DateTimeOffset";
                case "decimal":
                    return "Decimal";
                case "double":
                    return "Double";
                case "float":
                    return "Float";
                case "image":
                    return "Image";
                case "int":
                    return "Int32";
                case "money":
                    return "Money";
                case "nchar":
                    return "NChar";
                case "ntext":
                    return "NText";
                case "nvarchar":
                    return "NVarChar";
                case "real":
                    return "Real";
                case "smalldatetime":
                    return "SmallDateTime";
                case "smallint":
                    return isUnsigned ? "UInt16" : "Int16";
                case "smallmoney":
                    return "SmallMoney";
                case "text":
                    return "Text";
                case "time":
                    return "Time";
                case "timestamp":
                    return "Timestamp";
                case "tinyint":
                    return isUnsigned ? "UByte" : "Bit";
                case "uniqueidentifier":
                    return "UniqueIdentifier";
                case "varbinary":
                    return "VarBinary";
                case "varchar":
                    return "VarChar";
                case "variant":
                    return "Variant";
                case "xml":
                    return "Xml";
                case "year":
                    return "Year";
                case "mediumint":
                    return "Int24";
                case "enum":
                    return "Enum";
                case "set":
                    return "Set";
                case "numeric":
                    return "Numeric";
                case "longtext":
                    return "LongText";
                case "mediumtext":
                    return "MediumText";
                case "tinytext":
                    return "TinyText";
                case "tinyblob":
                    return "TinyBlob";
                case "mediumblob":
                    return "MediumBlob";
                case "longblob":
                    return "LongBlob";
                case "geometry":
                case "linestring":
                case "multilinestring":
                case "polygon":
                case "multipolygon":
                case "geometrycollection":
                case "point":
                case "multipoint":
                    return "Geometry";
            }

            return sqlType;
        }

        private string GetVbDataType(string mySqlDataType, bool isIdentity, bool isUnsigned)
        {
            switch (mySqlDataType.ToLower())
            {
                case "char":
                    return "Char";
                case "bigint":
                    return isUnsigned ? "ULong" : "Long";
                case "numeric":
                case "decimal":
                    return "Decimal";
                case "float":
                    return "Single";
                case "double":
                    return "Double";
                case "blob":
                case "binary":
                case "longblob":
                case "mediumblob":
                case "tinyblob":
                case "varbinary":
                    return "Byte()";
                case "tinyint":
                    return isUnsigned ? "Byte" : "SByte";
                case "smallint":
                    return isUnsigned ? "UShort" : "Short";
                case "int":
                case "year":
                case "mediumint":
                    return isUnsigned ? "UInteger" : "Integer";
                case "datetime":
                case "timestamp":
                case "date":
                    return "DateTime";
                case "time":
                    return "TimeSpan";
                case "bit":
                    return "Boolean";
                case "varchar":
                case "text":
                case "tinytext":
                case "longtext":
                case "enum":
                case "mediumtext":
                case "set":
                    return "String";
                case "geometry":
                case "geometrycollection":
                case "multipoint":
                case "multipolygon":
                case "point":
                case "polygon":
                case "linestring":
                case "multilinestring":
                    return "MySqlGeometry";
            }

            return mySqlDataType;
        }

        private string GetCsDataType(string mySqlDataType, bool isIdentity, bool isUnsigned)
        {
            switch (mySqlDataType.ToLower())
            {
                case "char":
                    return "char";
                case "bigint":
                    return isUnsigned ? "ulong" : "long";
                case "numeric":
                case "decimal":
                    return "decimal";
                case "float":
                    return "float";
                case "double":
                    return "double";
                case "blob":
                case "binary":
                case "longblob":
                case "mediumblob":
                case "tinyblob":
                case "varbinary":
                    return "byte[]";
                case "tinyint":
                    return isUnsigned ? "byte" : "sbyte";
                case "smallint":
                    return isUnsigned ? "ushort" : "short";
                case "int":
                case "year":
                case "mediumint":
                    return isUnsigned ? "uint" : "int";
                case "datetime":
                case "timestamp":
                case "date":
                    return "DateTime";
                case "time":
                    return "TimeSpan";
                case "bit":
                    return "bool";
                case "varchar":
                case "text":
                case "tinytext":
                case "longtext":
                case "enum":
                case "mediumtext":
                case "set":
                    return "string";
                case "geometry":
                case "geometrycollection":
                case "multipoint":
                case "multipolygon":
                case "point":
                case "polygon":
                case "linestring":
                case "multilinestring":
                    return "MySqlGeometry";
            }

            return mySqlDataType;
        }
    }
}
