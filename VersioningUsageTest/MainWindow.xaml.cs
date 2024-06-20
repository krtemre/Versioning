using System.Diagnostics;
using System.IO;
using System.Windows;
using Versioning;
using VersioningUsageTest.Classes;
using VersioningUsageTest.SaveLoad;

namespace VersioningUsageTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string fileLoc = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VersionSave");

        public MainWindow()
        {
            InitializeComponent();

            VersionDataController.SaveVersion();

            //Version 1
            //TestClassA saveObjectA = new TestClassA()
            //{
            //    VersionIntData = 2,
            //    NonVersionIntData = 1,
            //    VersionFloatData = 12f,
            //    VersionBData = new TestClassB()
            //    {
            //        VersionBoolData = true,
            //        VersionByteData = 2,
            //        VersionIntArrData = new int[] { 1, 2, 3 },
            //    },
            //};

            TestClassA saveObjectA = new TestClassA()
            {
                VersionIntData = 2,
                NonVersionIntData = 1,
                //VersionFloatData = 12f,
                VersionBData = new TestClassB()
                {
                    VersionBoolData = true,
                    //VersionByteData = 2,
                    VersionIntArrData = new int[] { 1, 2, 3 },
                },
            };

            //ByteSave.SaveObject(saveObjectA, fileLoc);
            //Debug.WriteLine("Object Saved");

            TestClassA loadedObjectA = new TestClassA();

            ByteLoad.LoadObject(loadedObjectA, fileLoc);
        }
    }
}