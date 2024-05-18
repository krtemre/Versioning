using Versioning;

namespace VersioningUsageTest.Classes
{
    public class TestClassB
    {
        [VersionProperty(VersionTypeEnum.BOOL)]
        public bool VersionBoolData { get; set; }

        [VersionProperty(VersionTypeEnum.BYTE)]
        public byte VersionByteData { get; set; }

        public int[] VersionIntArrData { get; set; }
    }
}
