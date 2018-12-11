using System.Text;

namespace Classicle.Implementations.Language
{
    internal class CSharp : ILanguage
    {
        public string FileNameExtension => ".cs";

        public string GetSafeFieldName(Field field)
        {
            return field.SafeCsFieldName;
        }

        public string GetSafePropertyName(Field field)
        {
            return field.SafeCsPropertyName;
        }

        public string GetImportsCode(bool useDapperExtensions)
        {
            if (useDapperExtensions)
            {
                return "using Dapper.Contrib.Extensions;";
            }

            return "";
        }

        public string GetNamespaceCode(string defaultNamespace)
        {
            return $"namespace {Utility.GetCsPropertyName(defaultNamespace)}";
        }

        public string GetOpeningNamespaceMarker()
        {
            return "{";
        }

        public string GetClosingNamespaceMarker()
        {
            return "}";
        }

        public string GetPublicClassCode(string objectName)
        {
            return $"public class {Utility.GetCsPropertyName(objectName)}";
        }

        public string GetOpeningClassMarker()
        {
            return "{";
        }

        public string GetClosingClassMarker()
        {
            return "}";
        }

        public string GetAttributeOpeningMarker()
        {
            return "[";
        }

        public string GetAttributeClosingMarker()
        {
            return "]";
        }

        public string GetSingleLineCommentMarker()
        {
            return "//";
        }

        public string GetPrivateBackingFieldCode(Field field)
        {
            string nullable = "";

            if (field.CanBeNull && field.CsDataType != "string") nullable = "?";

            return $"private {field.CsDataType}{nullable} {field.SafeCsFieldName};";
        }

        public string GetPublicPropertyCode(Field field, bool useBackingFields, int indentLength)
        {
            string nullable = "";

            if (field.CanBeNull && field.CsDataType != "string") nullable = "?";

            if (!useBackingFields)
            {
                return $"{Utility.GetIndentSpaces(indentLength)}public {field.CsDataType}{nullable} {field.SafeCsPropertyName} {{ get; set; }}";
            }

            var sb = new StringBuilder();

            sb.AppendLine($"{Utility.GetIndentSpaces(indentLength)}public {field.CsDataType}{nullable} {field.SafeCsPropertyName}");
            sb.AppendLine($"{Utility.GetIndentSpaces(indentLength)}{{");
            sb.AppendLine($"{Utility.GetIndentSpaces(indentLength + 4)}get {{ return {field.SafeCsFieldName}; }}");
            sb.AppendLine($"{Utility.GetIndentSpaces(indentLength + 4)}set {{ {field.SafeCsFieldName} = value; }}");
            sb.AppendLine($"{Utility.GetIndentSpaces(indentLength)}}}");

            return sb.ToString();
        }

        public string GetClassicalFile(string defaultNamespace, string connectionString)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"namespace {Utility.GetCsPropertyName(defaultNamespace)}");
            sb.AppendLine("{");
            sb.AppendLine("    public static class Classicle");
            sb.AppendLine("    {");
            sb.AppendLine("        public static string GetConnectionString()");
            sb.AppendLine("        {");
            sb.AppendLine("            return \"" + connectionString.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\";");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}
