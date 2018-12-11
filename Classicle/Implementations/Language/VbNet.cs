using System.Text;

namespace Classicle.Implementations.Language
{
    internal class VbNet : ILanguage
    {
        public string FileNameExtension => ".vb";

        public string GetSafeFieldName(Field field)
        {
            return field.SafeVbFieldName;
        }

        public string GetSafePropertyName(Field field)
        {
            return field.SafeVbPropertyName;
        }

        public string GetImportsCode(bool useDapperExtensions)
        {
            if (useDapperExtensions)
            {
                return "Imports Dapper.Contrib.Extensions";
            }

            return "";
        }

        public string GetNamespaceCode(string defaultNamespace)
        {
            return $"Namespace {Utility.GetVbPropertyName(defaultNamespace)}";
        }

        public string GetOpeningNamespaceMarker()
        {
            return "";
        }

        public string GetClosingNamespaceMarker()
        {
            return "End Namespace";
        }

        public string GetPublicClassCode(string objectName)
        {
            return $"Public Class {Utility.GetVbPropertyName(objectName)}";
        }

        public string GetOpeningClassMarker()
        {
            return "";
        }

        public string GetClosingClassMarker()
        {
            return "End Class";
        }

        public string GetAttributeOpeningMarker()
        {
            return "<";
        }

        public string GetAttributeClosingMarker()
        {
            return ">";
        }

        public string GetSingleLineCommentMarker()
        {
            return "'";
        }

        public string GetPrivateBackingFieldCode(Field field)
        {
            string nullable = "";

            if (field.CanBeNull && field.CsDataType != "string") nullable = "?";

            return $"Private {field.SafeVbFieldName} As {field.VbDataType}{nullable}";
        }

        public string GetPublicPropertyCode(Field field, bool useBackingFields, int indentLength)
        {
            string nullable = "";

            if (field.CanBeNull && field.CsDataType != "string") nullable = "?";

            if (!useBackingFields)
            {
                return $"{Utility.GetIndentSpaces(indentLength)}Public Property {field.SafeVbPropertyName} As {field.VbDataType}{nullable}";
            }

            var sb = new StringBuilder();

            sb.AppendLine($"{Utility.GetIndentSpaces(indentLength)}Public Property {field.SafeVbPropertyName} As {field.VbDataType}{nullable}");
            sb.AppendLine($"{Utility.GetIndentSpaces(indentLength + 4)}Get");
            sb.AppendLine($"{Utility.GetIndentSpaces(indentLength + 8)}Return {field.SafeVbFieldName}");
            sb.AppendLine($"{Utility.GetIndentSpaces(indentLength + 4)}End Get");
            sb.AppendLine($"{Utility.GetIndentSpaces(indentLength + 4)}Set(ByVal value As {field.VbDataType}{nullable})");
            sb.AppendLine($"{Utility.GetIndentSpaces(indentLength + 8)}{field.SafeVbFieldName} = value");
            sb.AppendLine($"{Utility.GetIndentSpaces(indentLength + 4)}End Set");
            sb.AppendLine($"{Utility.GetIndentSpaces(indentLength)}End Property");

            return sb.ToString();
        }

        public string GetClassicalFile(string defaultNamespace, string connectionString)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Namespace {Utility.GetVbPropertyName(defaultNamespace)}");
            sb.AppendLine("    Public Module Classicle");
            sb.AppendLine("        Function GetConnectionString() As String");
            sb.AppendLine("            Return \"" + connectionString.Replace("\"", "\"\"") + "\"");
            sb.AppendLine("        End Function");
            sb.AppendLine("    End Module");
            sb.AppendLine("End Namespace");

            return sb.ToString();
        }
    }
}
