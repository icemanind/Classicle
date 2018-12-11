using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace Classicle.Implementations.Database
{
    internal class SqlServer : IDatabase
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
                var builder = new SqlConnectionStringBuilder
                {
                    ["Data Source"] = ServerName + "," + ServerPort,
                    ["Integrated Security"] = TrustedConnection,
                    ["Initial Catalog"] = DatabaseName
                };

                if (!TrustedConnection)
                {
                    builder["User ID"] = Username;
                    builder["Password"] = Password;
                }

                return builder.ConnectionString;
            }
        }

        public SqlServer()
        {
            DefaultSchema = "dbo";
            ServerPort = 1433;
        }

        public List<ClassicleObject> GetTablesAndViews()
        {
            var cObjects = new List<ClassicleObject>();

            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = ConnectionString;
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "sp_tables";
                    command.Parameters.AddWithValue("@table_qualifier", DatabaseName);
                    command.Parameters.AddWithValue("@table_owner", DefaultSchema);

                    using (var adapter = new SqlDataAdapter())
                    {
                        adapter.SelectCommand = command;
                        using (var ds = new DataSet())
                        {
                            adapter.Fill(ds);
                            if (ds.Tables.Count > 0)
                            {
                                foreach (DataRow row in ds.Tables[0].Rows)
                                {
                                    var cObject = new ClassicleObject { ObjectName = (string)row["TABLE_NAME"] };
                                    if (cObject.ObjectName.ToLower() == "dtproperties" || cObject.ObjectName.ToLower() == "syscolumns" ||
                                        cObject.ObjectName.ToLower() == "sysdepends" || cObject.ObjectName.ToLower() == "syscomments" ||
                                        cObject.ObjectName.ToLower() == "sysfilegroups" || cObject.ObjectName.ToLower() == "sysfiles" ||
                                        cObject.ObjectName.ToLower() == "sysfiles1" || cObject.ObjectName.ToLower() == "sysforeignkeys" ||
                                        cObject.ObjectName.ToLower() == "sysproperties" || cObject.ObjectName.ToLower() == "sysusers" ||
                                        cObject.ObjectName.ToLower() == "sysconstraints" || cObject.ObjectName.ToLower() == "syssegments" ||
                                        cObject.ObjectName.ToLower() == "sysdiagrams")
                                    {
                                        continue;
                                    }

                                    cObject.IsView = ((string)row["TABLE_TYPE"]).ToLower() == "view";

                                    cObjects.Add(cObject);
                                }
                            }
                        }
                    }
                }
            }

            return cObjects;
        }

        public List<Field> MapFields(string tableName)
        {
            var fields = new List<Field>();

            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = ConnectionString;
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "sp_columns";
                    command.Parameters.AddWithValue("@table_name", tableName);
                    command.Parameters.AddWithValue("@table_owner", DefaultSchema);

                    using (var adapter = new SqlDataAdapter())
                    {
                        adapter.SelectCommand = command;
                        using (var ds = new DataSet())
                        {
                            adapter.Fill(ds);
                            if (ds.Tables.Count > 0)
                            {
                                foreach (DataRow row in ds.Tables[0].Rows)
                                {
                                    var field = new Field();

                                    field.FieldName = (string)row["COLUMN_NAME"];
                                    field.IsPrimaryKey = IsPrimaryKey(tableName, field.FieldName);
                                    field.IsIdentity = ((string)row["TYPE_NAME"]).ToLower().Trim().EndsWith("identity");
                                    field.CsDataType = GetCsDataType(((string)row["TYPE_NAME"]).Trim());
                                    field.VbDataType = GetVbDataType(((string)row["TYPE_NAME"]).Trim());
                                    field.SafeCsFieldName = Utility.GetSafeCsName(Utility.GetCsFieldName(field.FieldName));
                                    field.SafeCsPropertyName = Utility.GetSafeCsName(Utility.GetCsPropertyName(field.FieldName, Utility.GetSafeCsName(Utility.GetCsPropertyName(tableName))));
                                    field.SafeVbFieldName = Utility.GetSafeVbName(Utility.GetVbFieldName(field.FieldName));
                                    field.SafeVbPropertyName = Utility.GetSafeVbName(Utility.GetVbPropertyName(field.FieldName, Utility.GetSafeVbName(Utility.GetVbPropertyName(tableName))));
                                    field.SqlDbType = GetSqlDbTypeFromSqlType(((string)row["TYPE_NAME"]).Trim());
                                    field.IntrinsicSqlDataType = ((string)row["TYPE_NAME"]).ToUpper().Trim();
                                    field.IntrinsicSqlDataType = field.IntrinsicSqlDataType.Replace(" IDENTITY", "").Trim();
                                    field.IsValueType = Utility.IsValueType(field.CsDataType);
                                    field.DefaultValue = GetDefaultValue(((string)row["TYPE_NAME"]).Trim());

                                    try
                                    {
                                        field.SqlPrecision = (int)row["PRECISION"];
                                    }
                                    catch
                                    {
                                        field.SqlPrecision = 0;
                                    }
                                    try
                                    {
                                        field.SqlScale = (short)row["SCALE"];
                                    }
                                    catch
                                    {
                                        field.SqlScale = 0;
                                    }
                                    field.TextBased = IsTextBased(GetCsDataType(((string)row["TYPE_NAME"]).Trim()));
                                    field.Description = GetDescription(tableName, field.FieldName);
                                    field.IsComputedField = IsFieldComputed(tableName, field.FieldName);
                                    field.CanBeNull = ((short)row["NULLABLE"]) == 1;
                                    fields.Add(field);
                                }
                            }
                        }
                    }
                }
            }

            return fields;
        }

        private string GetCsDataType(string sqlTypeName)
        {
            sqlTypeName = sqlTypeName.EndsWith("identity") ? sqlTypeName.Substring(0, sqlTypeName.Length - 8) : sqlTypeName;
            sqlTypeName = sqlTypeName.Trim().ToLower();

            switch (sqlTypeName)
            {
                case "bigint":
                    return "long";
                case "binary":
                case "image":
                case "rowversion":
                case "timestamp":
                case "varbinary":
                    return "byte[]";
                case "bit":
                    return "bool";
                case "char":
                case "nchar":
                case "ntext":
                case "nvarchar":
                case "text":
                case "varchar":
                    return "string";
                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                    return "System.DateTime";
                case "datetimeoffset":
                    return "System.DateTimeOffset";
                case "decimal":
                case "money":
                case "numeric":
                case "smallmoney":
                    return "decimal";
                case "float":
                    return "double";
                case "real":
                    return "float";
                case "int":
                    return "int";
                case "smallint":
                    return "short";
                case "sql_variant":
                    return "object";
                case "time":
                    return "System.TimeSpan";
                case "tinyint":
                    return "byte";
                case "uniqueidentifier":
                    return "System.Guid";
                case "xml":
                    return "string";
            }

            return "object";
        }

        private string GetVbDataType(string sqlTypeName)
        {
            sqlTypeName = sqlTypeName.EndsWith("identity") ? sqlTypeName.Substring(0, sqlTypeName.Length - 8) : sqlTypeName;
            sqlTypeName = sqlTypeName.Trim().ToLower();

            switch (sqlTypeName)
            {
                case "bigint":
                    return "Long";
                case "binary":
                case "image":
                case "rowversion":
                case "timestamp":
                case "varbinary":
                    return "Byte()";
                case "bit":
                    return "Boolean";
                case "char":
                case "nchar":
                case "ntext":
                case "nvarchar":
                case "text":
                case "varchar":
                    return "String";
                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                    return "System.DateTime";
                case "datetimeoffset":
                    return "System.DateTimeOffset";
                case "decimal":
                case "money":
                case "numeric":
                case "smallmoney":
                    return "Decimal";
                case "float":
                    return "Double";
                case "real":
                    return "Single";
                case "int":
                    return "Integer";
                case "smallint":
                    return "Short";
                case "sql_variant":
                    return "Object";
                case "time":
                    return "System.TimeSpan";
                case "tinyint":
                    return "Byte";
                case "uniqueidentifier":
                    return "System.Guid";
                case "xml":
                    return "string";
            }

            return "object";
        }

        private string GetDefaultValue(string sqlTypeName)
        {
            sqlTypeName = sqlTypeName.EndsWith("identity") ? sqlTypeName.Substring(0, sqlTypeName.Length - 8) : sqlTypeName;
            sqlTypeName = sqlTypeName.Trim().ToLower();

            switch (sqlTypeName)
            {
                case "bit":
                    return "false";
                case "char":
                case "nchar":
                case "ntext":
                case "nvarchar":
                case "text":
                case "varchar":
                    return "\"\"";
                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                    return "DateTime.Now";
                case "datetimeoffset":
                    return "DateTimeOffset.Now";
                case "decimal":
                case "money":
                case "numeric":
                case "smallmoney":
                case "float":
                case "real":
                case "int":
                case "smallint":
                case "tinyint":
                case "varbinary":
                case "bigint":
                case "binary":
                case "image":
                case "rowversion":
                case "timestamp":
                    return "0";
                case "time":
                    return "TimeSpan.Now";
                case "uniqueidentifier":
                    return "new Guid()";
                case "xml":
                case "sql_variant":
                    return "null";
            }

            return "object";
        }

        private bool IsPrimaryKey(string tableName, string fieldName)
        {
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = ConnectionString;
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "sp_pkeys";
                    command.Parameters.AddWithValue("@table_name", tableName);
                    command.Parameters.AddWithValue("@table_owner", DefaultSchema);

                    using (var adapter = new SqlDataAdapter())
                    {
                        adapter.SelectCommand = command;
                        using (var ds = new DataSet())
                        {
                            adapter.Fill(ds);
                            if (ds.Tables.Count > 0)
                            {
                                if (ds.Tables[0].Rows.Cast<DataRow>().Any(row => ((string)row["COLUMN_NAME"]).ToLower() == fieldName.ToLower()))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool IsFieldComputed(string tableName, string fieldName)
        {
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = ConnectionString;
                using (var command = new SqlCommand())
                {
                    string sql = "SELECT sysobjects.name AS TableName, syscolumns.name AS ColumnName FROM syscolumns INNER JOIN sysobjects ON syscolumns.id = sysobjects.id";
                    sql = sql + " AND sysobjects.xtype = 'U' WHERE syscolumns.iscomputed = 1 AND sysobjects.name = '" + tableName + "'";
                    sql = sql + " AND syscolumns.name = '" + fieldName + "'";

                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = sql;

                    using (var adapter = new SqlDataAdapter())
                    {
                        adapter.SelectCommand = command;
                        using (var ds = new DataSet())
                        {
                            adapter.Fill(ds);
                            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                            {
                                if (ds.Tables[0].Rows[0]["ColumnName"] == null || ds.Tables[0].Rows[0]["ColumnName"] == DBNull.Value)
                                    return false;

                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private string GetSqlDbTypeFromSqlType(string sqlType)
        {
            sqlType = sqlType.EndsWith("identity") ? sqlType.Substring(0, sqlType.Length - 8) : sqlType;
            sqlType = sqlType.Trim().ToLower();

            switch (sqlType)
            {
                case "bigint":
                    return "BigInt";
                case "binary":
                    return "Binary";
                case "bit":
                    return "Bit";
                case "char":
                    return "Char";
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
                case "float":
                    return "Float";
                case "image":
                    return "Image";
                case "int":
                    return "Int";
                case "money":
                    return "Money";
                case "nchar":
                    return "NChar";
                case "ntext":
                    return "NText";
                case "numeric":
                    return "Decimal";
                case "nvarchar":
                    return "NVarChar";
                case "real":
                    return "Real";
                case "smalldatetime":
                    return "SmallDateTime";
                case "smallint":
                    return "SmallInt";
                case "smallmoney":
                    return "SmallMoney";
                case "text":
                    return "Text";
                case "time":
                    return "Time";
                case "timestamp":
                    return "Timestamp";
                case "tinyint":
                    return "TinyInt";
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
            }

            return sqlType;
        }

        private bool IsTextBased(string csType)
        {
            return csType.ToLower() == "string";
        }

        private string GetDescription(string tableName, string fieldName)
        {
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = ConnectionString;
                using (var command = new SqlCommand())
                {
                    string sql = "SELECT  st.name AS [Table] , sc.name AS [Column] ,sep.value AS [Description] FROM sys.tables st";
                    sql = sql + " INNER JOIN sys.columns sc ON st.object_id = sc.object_id LEFT JOIN sys.extended_properties sep ON st.object_id = sep.major_id AND sc.column_id = sep.minor_id AND sep.name = 'MS_Description'";
                    sql = sql + " WHERE st.name = '" + tableName + "' AND sc.name = '" + fieldName + "'";

                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = sql;

                    using (var adapter = new SqlDataAdapter())
                    {
                        adapter.SelectCommand = command;
                        using (var ds = new DataSet())
                        {
                            adapter.Fill(ds);
                            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                            {
                                if (ds.Tables[0].Rows[0]["Description"] == null || ds.Tables[0].Rows[0]["Description"] == DBNull.Value)
                                    return "";
                                var description = (string)ds.Tables[0].Rows[0]["Description"];

                                return string.IsNullOrEmpty(description) ? "" : description;
                            }
                        }
                    }
                }
            }
            return "";
        }

        public void CreateLayers(List<ObjectViewModel> objects, string outputFolder)
        {
            ILanguage language = null;

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
                string fileName = Utility.GetCsPropertyName(obj.ObjectName);
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
                    } else if (field.IsPrimaryKey && UseDapperExtensions)
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
    }
}
