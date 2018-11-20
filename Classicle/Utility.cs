using System.Text.RegularExpressions;

namespace Classicle
{
    internal static class Utility
    {
        internal static bool IsValueType(string csDataType)
        {
            switch (csDataType.ToLower())
            {
                case "bool":
                case "byte":
                case "char":
                case "datetime":
                case "decimal":
                case "double":
                case "float":
                case "guid":
                case "int":
                case "long":
                case "sbyte":
                case "short":
                case "timespan":
                case "uint":
                case "ulong":
                case "ushort":
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Converts a string containing a name into a name safely
        /// suited for VB.Net. It does this by converting spaces into
        /// underscores and wraps the name in square brackets if its a reserved
        /// VB.Net keyword.
        /// </summary>
        /// <param name="fieldName">The string containing the name</param>
        /// <returns>The safe VB.Net name.</returns>
        internal static string GetSafeVbName(string fieldName)
        {
            string originalName = fieldName;
            fieldName = fieldName.ToLower();

            if (fieldName == "addhandler" || fieldName == "addressof" || fieldName == "alias" || fieldName == "and" ||
                fieldName == "andalso" || fieldName == "as" || fieldName == "boolean" || fieldName == "byref" ||
                fieldName == "byte" || fieldName == "byval" || fieldName == "call" || fieldName == "case" ||
                fieldName == "catch" || fieldName == "cbool" || fieldName == "cbyte" || fieldName == "cchar" ||
                fieldName == "cdate" || fieldName == "cdec" || fieldName == "cdbl" || fieldName == "char" ||
                fieldName == "cint" || fieldName == "class" || fieldName == "clng" || fieldName == "cobj" ||
                fieldName == "const" || fieldName == "continue" || fieldName == "csbyte" || fieldName == "cshort" ||
                fieldName == "csng" || fieldName == "cstr" || fieldName == "ctype" || fieldName == "cuint" ||
                fieldName == "culng" || fieldName == "cushort" || fieldName == "date" || fieldName == "decimal" ||
                fieldName == "declare" || fieldName == "default" || fieldName == "delegate" || fieldName == "dim" ||
                fieldName == "directcast" || fieldName == "do" || fieldName == "double" || fieldName == "each" ||
                fieldName == "else" || fieldName == "elseif" || fieldName == "end" || fieldName == "endif" ||
                fieldName == "enum" || fieldName == "erase" || fieldName == "error" || fieldName == "event" ||
                fieldName == "exit" || fieldName == "false" || fieldName == "finally" || fieldName == "for" ||
                fieldName == "friend" || fieldName == "function" || fieldName == "get" || fieldName == "gettype" ||
                fieldName == "getxmlnamespace" || fieldName == "global" || fieldName == "gosub" || fieldName == "goto" ||
                fieldName == "handles" || fieldName == "if" || fieldName == "implements" || fieldName == "imports" ||
                fieldName == "in" || fieldName == "inherits" || fieldName == "integer" || fieldName == "interface" ||
                fieldName == "is" || fieldName == "isnot" || fieldName == "let" || fieldName == "lib" ||
                fieldName == "like" || fieldName == "long" || fieldName == "loop" || fieldName == "me" ||
                fieldName == "mod" || fieldName == "module" || fieldName == "mustinherit" || fieldName == "mustoverride" ||
                fieldName == "mybase" || fieldName == "myclass" || fieldName == "namespace" || fieldName == "narrowing" ||
                fieldName == "new" || fieldName == "next" || fieldName == "not" || fieldName == "nothing" ||
                fieldName == "notinheritable" || fieldName == "notoverridable" || fieldName == "object" ||
                fieldName == "of" || fieldName == "on" || fieldName == "operator" || fieldName == "option" ||
                fieldName == "optional" || fieldName == "or" || fieldName == "orelse" || fieldName == "overloads" ||
                fieldName == "overridable" || fieldName == "overrides" || fieldName == "paramarray" || fieldName == "partial" ||
                fieldName == "private" || fieldName == "property" || fieldName == "protected" || fieldName == "public" ||
                fieldName == "raiseevent" || fieldName == "readonly" || fieldName == "redim" ||
                fieldName == "rem" || fieldName == "removehandler" || fieldName == "resume" || fieldName == "return" ||
                fieldName == "sbyte" || fieldName == "select" || fieldName == "set" || fieldName == "shadows" ||
                fieldName == "shared" || fieldName == "short" || fieldName == "single" || fieldName == "static" ||
                fieldName == "step" || fieldName == "stop" || fieldName == "string" || fieldName == "structure" ||
                fieldName == "sub" || fieldName == "synclock" || fieldName == "then" || fieldName == "throw" ||
                fieldName == "to" || fieldName == "true" || fieldName == "try" || fieldName == "trycast" ||
                fieldName == "typeof" || fieldName == "variant" || fieldName == "wend" || fieldName == "uinteger" ||
                fieldName == "ulong" || fieldName == "ushort" || fieldName == "using" || fieldName == "when" ||
                fieldName == "while" || fieldName == "widening" || fieldName == "with" || fieldName == "withevents" ||
                fieldName == "writeonly" || fieldName == "xor")
            {
                return "[" + originalName.Replace(" ", "_") + "]";
            }

            return originalName.Replace(" ", "_");
        }

        /// <summary>
        /// Converts a string containing a name into a name safely
        /// suited for C#. It does this by converting spaces into
        /// underscores and prefixes the name with an at (@) sign if its
        /// a reserved C# keyword.
        /// </summary>
        /// <param name="fieldName">The string containing the name</param>
        /// <returns>The safe C# name.</returns>
        internal static string GetSafeCsName(string fieldName)
        {
            string originalName = fieldName;
            fieldName = fieldName.ToLower();

            if (fieldName == "abstract" || fieldName == "as" || fieldName == "base" || fieldName == "bool" ||
                fieldName == "break" || fieldName == "byte" || fieldName == "case" || fieldName == "catch" ||
                fieldName == "char" || fieldName == "checked" || fieldName == "class" || fieldName == "const" ||
                fieldName == "continue" || fieldName == "decimal" || fieldName == "default" || fieldName == "delegate" ||
                fieldName == "do" || fieldName == "double" || fieldName == "else" || fieldName == "enum" ||
                fieldName == "event" || fieldName == "explicit" || fieldName == "extern" || fieldName == "false" ||
                fieldName == "finally" || fieldName == "fixed" || fieldName == "float" || fieldName == "for" ||
                fieldName == "foreach" || fieldName == "goto" || fieldName == "if" || fieldName == "implicit" ||
                fieldName == "in" || fieldName == "int" || fieldName == "interface" || fieldName == "internal" ||
                fieldName == "is" || fieldName == "lock" || fieldName == "long" || fieldName == "namespace" ||
                fieldName == "new" || fieldName == "null" || fieldName == "object" || fieldName == "operator" ||
                fieldName == "out" || fieldName == "override" || fieldName == "params" || fieldName == "private" ||
                fieldName == "protected" || fieldName == "public" || fieldName == "readonly" || fieldName == "ref" ||
                fieldName == "return" || fieldName == "sbyte" || fieldName == "sealed" || fieldName == "short" ||
                fieldName == "sizeof" || fieldName == "stackalloc" || fieldName == "static" || fieldName == "string" ||
                fieldName == "struct" || fieldName == "switch" || fieldName == "this" || fieldName == "throw" ||
                fieldName == "true" || fieldName == "try" || fieldName == "typeof" || fieldName == "uint" ||
                fieldName == "ulong" || fieldName == "unchecked" || fieldName == "unsafe" || fieldName == "ushort" ||
                fieldName == "using" || fieldName == "yield" || fieldName == "virtual" || fieldName == "void" ||
                fieldName == "volatile" || fieldName == "while")
            {
                return "@" + originalName.Replace(" ", "_");
            }

            return originalName.Replace(" ", "_");
        }

        internal static string GetCsPropertyName(string fieldName, string className)
        {
            if (GetCsPropertyName(fieldName) == className)
                return GetCsPropertyName(fieldName) + "_";
            return GetCsPropertyName(fieldName);
        }

        internal static string GetCsPropertyName(string fieldName)
        {
            return GetSafeCsName(GetPascalCaseName(fieldName));
        }

        internal static string GetCsFieldName(string fieldName)
        {
            return "_" + GetCamelCaseName(fieldName);
        }

        internal static string GetVbFieldName(string fieldName)
        {
            return "_" + GetCamelCaseName(fieldName);
        }

        internal static string GetVbPropertyName(string fieldName)
        {
            return GetSafeVbName(GetPascalCaseName(fieldName));
        }

        internal static string GetVbPropertyName(string fieldName, string className)
        {
            if (GetVbPropertyName(fieldName) == className)
                return GetVbPropertyName(fieldName) + "_";
            return GetVbPropertyName(fieldName);
        }

        internal static string ReplaceNumber(string name)
        {
            string num = "";


            if (string.IsNullOrWhiteSpace(name)) return "";

            if (char.IsNumber(name[0]))
            {
                switch (name[0])
                {
                    case '0':
                        num = "Zero";
                        break;
                    case '1':
                        num = "One";
                        break;
                    case '2':
                        num = "Two";
                        break;
                    case '3':
                        num = "Three";
                        break;
                    case '4':
                        num = "Four";
                        break;
                    case '5':
                        num = "Five";
                        break;
                    case '6':
                        num = "Six";
                        break;
                    case '7':
                        num = "Seven";
                        break;
                    case '8':
                        num = "Eight";
                        break;
                    case '9':
                        num = "Nine";
                        break;
                }

                name = num + name.Substring(1);
            }

            return name;
        }

        internal static string GetPascalCaseName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            name = name.Trim();
            if (name.Length == 1) return char.ToUpperInvariant(name[0]) + "";
            name = Regex.Replace(name, "(?<=[a-z])(?=[A-Z])", " ");
            string s = System.Globalization.CultureInfo.CurrentCulture.
                TextInfo.ToTitleCase(name.ToLower()).Replace(" ", "").Replace("_", "");

            return ReplaceNumber(s.Replace("<", "").Replace(">", "").Replace("?", "").Replace(".", "").Replace(",", "")
                    .Replace("&", "").Replace("$", "").Replace("!", "").Replace("@", "").Replace("%", "")
                    .Replace("^", "").Replace("(", "").Replace(")", "").Replace("+", "").Replace("/", "")
                    .Replace("-", "").Replace("=", "").Replace("*", "").Replace("[", "").Replace("]", "")
                    .Replace("{", "").Replace("}", ""));
        }

        internal static string GetCamelCaseName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            name = name.Trim();

            if (name.Length == 1) return char.ToLowerInvariant(name[0]) + "";
            return char.ToLowerInvariant(GetPascalCaseName(name)[0]) + GetPascalCaseName(name).Substring(1);
        }
    }
}
