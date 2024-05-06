namespace Versioning
{
    //Use this attribute for the class that u want to save its properties
    public sealed class VersionClassAttribute : Attribute
    {
        public ushort Version { get; set; }

        public VersionClassAttribute(ushort version)
        {
            Version = version;
        }
    }

    public enum VersionTypeEnums : byte
    {
        BYTE,
        SBYTE,
        BOOL,
        CHAR,
        SHORT,
        USHORT,
        UINT,
        INT,
        LONG,
        ULONG,
        DECIMAL,
        FLOAT,
        DOUBLE,
        STRING,
        DATETIME,
        OBJECT,
    }

    //Add this to the property which u want to save its data other wise it wont be saved by code.
    public sealed class VersionPropertyAttribute : Attribute
    {
        public VersionTypeEnums TypeEnum { get; set; }

        public VersionPropertyAttribute(VersionTypeEnums typeEnum)
        {
            TypeEnum = typeEnum;
        }
    }
}
