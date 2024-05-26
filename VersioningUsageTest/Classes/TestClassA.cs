using Versioning;

namespace VersioningUsageTest.Classes
{
    [VersionClassAttribute] //Use this attribute as an entry point to saving version.
    public class TestClassA
    {
        [VersionPropertyAttribute(VersionValueTypeEnum.INT)]
        public int VersionIntData { get; set; }

        public int NonVersionIntData { get; set; }


        [VersionPropertyAttribute(VersionValueTypeEnum.FLOAT)]
        public float VersionFloatData { get; set; }

        [VersionPropertyAttribute(VersionValueTypeEnum.OBJECT)]
        public TestClassB VersionBData { get; set; }
    }
}
