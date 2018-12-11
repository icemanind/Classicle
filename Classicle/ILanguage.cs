namespace Classicle
{
    internal interface ILanguage
    {
        string FileNameExtension { get; }
        string GetImportsCode(bool useDapperExtensions);
        string GetNamespaceCode(string defaultNamespace);
        string GetOpeningNamespaceMarker();
        string GetClosingNamespaceMarker();
        string GetPublicClassCode(string objectName);
        string GetOpeningClassMarker();
        string GetClosingClassMarker();
        string GetAttributeOpeningMarker();
        string GetAttributeClosingMarker();
        string GetSingleLineCommentMarker();
        string GetPrivateBackingFieldCode(Field field);
        string GetPublicPropertyCode(Field field, bool useBackingFields, int indentLength);
        string GetSafeFieldName(Field field);
        string GetSafePropertyName(Field field);
        string GetClassicalFile(string defaultNamespace, string connectionString);
    }
}
