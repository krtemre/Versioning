using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Versioning
{
    public class VersionData
    {
        public ushort VersionNumber;
        public List<VersionInformationData> VersionClasses;
    }

    public class VersionInformationData
    {
        public string Name;
        public string? ClassFullName;
        public List<SubVersionInformationData> Properties;
    }

    public class SubVersionInformationData
    {
        public string PropertyName;
        public string? ClassFullName;
        public string? PropertyClassFullName;
        public bool IsList;
        public bool IsArray;
        public VersionValueTypeEnum ValueType;
    }
}
