using System.Diagnostics;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace Versioning
{
    public static class VersionDataController
    {
        /// <summary>
        /// This directory indicates where the version data will hold its like AppDomain.CurrentDomain.BaseDirectory + DataDirectory be aware.
        /// </summary>
        public static string VersionDataDirectory = "Versions";

        private static List<VersionData> VersionsDataList;

        #region Save
        private static VersionData CurrentVersionData;

        /// <summary>
        /// Save class property which has VersionClassAttribute with a VersionPropertyAttribute.
        /// </summary>
        public static void SaveVersion(bool ifExistsDontSave = true)
        {
            if (VersionsDataList == null) { LoadVersions(); }

            if (ifExistsDontSave && VersionsDataList.Exists(s => s.VersionNumber == VersionManager.VersionNumber))
            {
                return;
            }

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, VersionDataDirectory);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string filePath = Path.Combine(path, $"Version-{VersionManager.VersionNumber}.xml");

            CurrentVersionData = new VersionData()
            {
                VersionNumber = VersionManager.VersionNumber,
                VersionClasses = new List<VersionInformationData>(),
            };

            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            List<Type> classTypes = new List<Type>();

            foreach (var assembly in allAssemblies)
            {
                //Get assembly classes as type
                foreach (Type t in assembly.GetTypes())
                {
                    var attributes = t.GetCustomAttributes(typeof(VersionClassAttribute), true);
                    //Control if the class type has necessary attribute
                    if (attributes != null && attributes.Length > 0)
                    {
                        classTypes.Add(t);
                    }
                }
            }

            foreach (var type in classTypes)
            {
                //Get class save properties if any of them is an object get its properties also as new class
                var versionClass = GetClassVersion(type);
                CurrentVersionData.VersionClasses.Add(versionClass);
            }

            //You can use your encryption to save version class.
            var xml = SerializeToXML(CurrentVersionData);
            File.WriteAllText(filePath, xml);
        }

        private static VersionInformationData GetClassVersion(Type t)
        {
            VersionInformationData versionInformationData = new VersionInformationData()
            {
                ClassFullName = t.FullName,
                Name = t.Name,
                Properties = new List<SubVersionInformationData>()
            };

            var propAll = t.GetProperties();

            foreach (var prop in propAll)
            {
                var attributes = (VersionPropertyAttribute[])prop.GetCustomAttributes(typeof(VersionPropertyAttribute), true);

                //Control attributes if it has an attribute(s) of VersionPropertyAttributes control return value
                if (attributes != null && attributes.Length > 0)
                {
                    var attribute = (VersionPropertyAttribute)attributes[0];

                    //Control if it is already exists
                    if (CurrentVersionData.VersionClasses.Exists(s => s.ClassFullName == prop.PropertyType.FullName))
                        continue;

                    versionInformationData.Properties.Add(GetPropertyVersionData(prop, attribute.TypeEnum));

                    //Control if attribute type is object if it is then update added classes
                    if (attribute.TypeEnum == VersionTypeEnum.OBJECT)
                    {
                        Type objClassType = prop.PropertyType;

                        //Control if it is list
                        if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            objClassType = objClassType.GetGenericArguments().Single();
                        }
                        else if (prop.PropertyType.IsArray)
                        {
                            objClassType = prop.PropertyType.GetElementType();
                        }

                        //Control if it is already exists if not add
                        if (objClassType != null && !CurrentVersionData.VersionClasses.Exists(s => s.ClassFullName == objClassType.FullName))
                        {
                            CurrentVersionData.VersionClasses.Add(GetClassVersion(objClassType));
                        }
                    }
                }
            }

            return versionInformationData;
        }

        private static SubVersionInformationData GetPropertyVersionData(PropertyInfo propertyInfo, VersionTypeEnum valueType)
        {
            SubVersionInformationData returnVal = new SubVersionInformationData()
            {
                PropertyName = propertyInfo.Name,
                PropertyClassFullName = propertyInfo.PropertyType.FullName,
                ValueType = valueType,
                IsList = false,
                IsArray = false,
            };

            //Control if property info is list, array or single value
            if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                returnVal.IsList = true;
                //Write list elements class name
                returnVal.ClassFullName = propertyInfo.PropertyType.GetGenericArguments().Single().FullName;
            }
            else if (propertyInfo.PropertyType.IsArray)
            {
                returnVal.IsArray = true;
                //Write array elements class name
                returnVal.ClassFullName = propertyInfo.PropertyType.GetElementType()?.FullName;
            }
            else
            {
                returnVal.ClassFullName = propertyInfo.PropertyType.FullName;
            }

            return returnVal;
        }
        #endregion

        #region Load
        public static void LoadVersions()
        {
            VersionsDataList?.Clear();
            VersionsDataList = new List<VersionData>();

            ushort versionNumber = VersionManager.VersionNumber;

            string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, VersionDataDirectory);

            if (!Directory.Exists(directory))
                return;

            while(versionNumber > 0)
            {
                string filePath = Path.Combine(directory, $"Version-{versionNumber}.xml");
                versionNumber--;

                if (!File.Exists(filePath))
                    continue;

                //If you encrypted be sure deserialize decrypted data
                VersionData? versionData = DeserializeFromXML<VersionData>(File.ReadAllText(filePath));

                if(versionData != null)
                    VersionsDataList.Add(versionData);
            }
        }
        #endregion

        #region XML
        public static string SerializeToXML(object obj)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());

                using (StringWriter textWrite = new StringWriter())
                {
                    xmlSerializer.Serialize(textWrite, obj);
                    return textWrite.ToString();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("An Error Occurred While Serialization, exepection : " + ex.ToString());

                return string.Empty;
            }
        }

        public static T? DeserializeFromXML<T>(string serializedData) where T : class
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                using (TextReader textReader = new StringReader(serializedData))
                {
                    return (T?)xmlSerializer.Deserialize(textReader);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("An Error Occurred While Deserialization, exepection : " + ex.ToString());

                return null;
            }
        }
        #endregion
    }
}
