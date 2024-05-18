using Versioning;

namespace VersioningUsageTest.Classes
{
    [VersionClassAttribute] //Use this attribute as an entry point to saving version.
    public class TestClassA
    {
        [VersionPropertyAttribute(VersionTypeEnum.INT)]
        public int VersionIntData { get; set; }

        public int NonVersionIntData { get; set; }


        [VersionPropertyAttribute(VersionTypeEnum.FLOAT)]
        public int VersionFloatData { get; set; }

        [VersionPropertyAttribute(VersionTypeEnum.OBJECT)]
        public TestClassB VersionBData { get; set; }
    }
}
