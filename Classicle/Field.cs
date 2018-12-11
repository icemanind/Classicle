namespace Classicle
{
    internal class Field
    {
        public string FieldName { get; set; }
        public string CsDataType { get; set; }
        public string VbDataType { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsIdentity { get; set; }
        public string SafeCsPropertyName { get; set; }
        public string SafeVbPropertyName { get; set; }
        public string SafeCsFieldName { get; set; }
        public string SafeVbFieldName { get; set; }
        public string SqlDbType { get; set; }
        public string IntrinsicSqlDataType { get; set; }
        public short SqlScale { get; set; }
        public int SqlPrecision { get; set; }
        public bool TextBased { get; set; }
        public string Description { get; set; }
        public bool IsComputedField { get; set; }
        public bool CanBeNull { get; set; }
        public bool IsValueType { get; set; }
        public string DefaultValue { get; set; }
        public bool IsUnsigned { get; set; }
    }
}
