namespace Versioning
{
    //Use this attribute for the class that u want to save its properties
    public sealed class VersionClassAttribute : Attribute
    {
        public ushort Version { get; private set; }

        public VersionClassAttribute()
        {
            Version = VersionManager.VersionNumber;
        }
    }

    public enum VersionTypeEnum : byte
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
        public VersionTypeEnum TypeEnum { get; set; }

        public VersionPropertyAttribute(VersionTypeEnum typeEnum)
        {
            TypeEnum = typeEnum;
        }
    }
}
