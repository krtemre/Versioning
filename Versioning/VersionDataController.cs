using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Versioning
{
    public sealed class VersionDataController
    {
        #region Singelton
        private static object _lockObj = new object();
        private static VersionDataController? _instance = null;

        public static VersionDataController Instance
        {
            get
            {
                lock (_lockObj)
                {
                    if (_instance == null)
                    {
                        _instance = new VersionDataController();
                    }
                    return _instance;
                }
            }
        }

        private VersionDataController()
        {

        }
        #endregion

        /// <summary>
        /// This directory indicates where the version data will hold its like AppDomain.CurrentDomain.BaseDirectory + DataDirectory be aware.
        /// </summary>
        public static string VersionDataDirectory = "Versions";

        private Dictionary<int, VersionInformationData> VersionsData;

        #region Save
        public void SaveVersion()
        {

        }
        #endregion

        #region Load
        public void LoadVersions()
        {

        }
        #endregion
    }
}
