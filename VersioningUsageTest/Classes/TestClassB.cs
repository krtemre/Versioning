using Versioning;

namespace VersioningUsageTest.Classes
{
    public class TestClassB
    {
        [VersionProperty(VersionValueTypeEnum.BOOL)]
        public bool VersionBoolData { get; set; }

        [VersionProperty(VersionValueTypeEnum.BYTE)]
        public byte VersionByteData { get; set; }

        [VersionProperty(VersionValueTypeEnum.INT)]
        public int[] VersionIntArrData { get; set; }
    }
}
