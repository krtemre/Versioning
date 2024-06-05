using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Xaml;
using Versioning;

namespace VersioningUsageTest.SaveLoad
{
    public static class ByteLoad
    {
        /// <summary>
        /// Loads the object in given xml path, the saved value must be object type.
        /// </summary>
        /// <param name="obj">Wanted obj also indicates saved object</param>
        /// <param name="path">Saved object xml path</param>
        /// <returns>Returns Loaded obj</returns>
        public static object LoadObject(object obj, string path)
        {
            if (!File.Exists(path)) { return new object(); }

            FileStream fileStream = new FileStream(path, FileMode.Open);
            BinaryReader reader = new BinaryReader(fileStream);

            ushort saveObjVersion = (ushort)reader.ReadInt16();

            int length = reader.ReadInt32();

            byte[] data = reader.ReadBytes(length);

            return ParseObject(obj, data, saveObjVersion);
        }

        /// <summary>
        /// Entery point of parsing no matter if it is current version or old version
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="data"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static object ParseObject(object instance, byte[] data, ushort version)
        {
            int index = 0;

            //If it is same version then parse directly else use version data
            if (version == VersionManager.VersionNumber)
            {
                return GetObject(instance, data, ref index);
            }
            else
            {
                return GetVersionedObject(instance, data, ref index, version);
            }
        }

        #region Same Version
        /// <summary>
        /// It is a main parsing function its parse object.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static object GetObject(object instance, byte[] data, ref int index)
        {
            Type type = instance.GetType();
            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                var attributes = (VersionPropertyAttribute[])property.GetCustomAttributes(typeof(VersionPropertyAttribute), false);

                if (attributes.Length > 0)
                {
                    var att = attributes[0];

                    SetObjectProperty(instance, data, ref index, property, att.TypeEnum, true);
                }
            }

            return instance;
        }

        /// <summary>
        /// Set the object property all to way bottom.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="property"></param>
        /// <param name="valueType"></param>
        /// <param name="fromInstance"></param>
        /// <returns></returns>
        private static object SetObjectProperty(object instance, byte[] data, ref int index, PropertyInfo property, VersionValueTypeEnum valueType, bool fromInstance)
        {
            object returnVal = null;

            if (fromInstance)
            {
                bool isList = Utils.IsList(property);
                bool isArray = property.PropertyType.IsArray;

                if (isList)
                {
                    var listType = typeof(List<>);
                    Type listElementType = property.PropertyType.GetGenericArguments().Single();
                    var constructedList = listType.MakeGenericType(listElementType);
                    object list = Activator.CreateInstance(constructedList);

                    int length = GetArrayOrListLength(ref index, data);

                    IList propVal = (IList)property.GetValue(instance, null);

                    for (int i = 0; i < length; i++)
                    {
                        object val = null;

                        if (propVal != null && i < propVal.Count && propVal[i] != null)
                        {
                            val = propVal[i];
                        }
                        else
                        {
                            val = Activator.CreateInstance(listElementType);
                        }

                        ((IList)list).Add(val);
                        var obj = SetObjectProperty(((IList)list)[i], data, ref index, property, valueType, false);

                        if (obj != null)
                            ((IList)list)[i] = obj;
                    }

                    property.SetValue(instance, (IList)list, null);
                }
                else if (isArray)
                {
                    var listType = typeof(List<>);
                    Type arrayElementType = property.PropertyType.GetElementType();
                    var constructedList = listType.MakeGenericType(arrayElementType);
                    object list = Activator.CreateInstance(constructedList);

                    IList propVal = (IList)property.GetValue(instance, null);

                    int length = GetArrayOrListLength(ref index, data);

                    for (int i = 0; i < length; i++)
                    {
                        object val = null;

                        if (propVal != null && i < propVal.Count && propVal[i] != null)
                        {
                            val = propVal[i];
                        }
                        else
                        {
                            val = Activator.CreateInstance(arrayElementType);
                        }

                        ((IList)list).Add(val);
                        var obj = SetObjectProperty(((IList)list)[i], data, ref index, property, valueType, false);

                        if (obj != null)
                            ((IList)list)[i] = obj;
                    }
                    Array array = Array.CreateInstance(arrayElementType, length);
                    for (int i = 0; i < length; i++)
                    {
                        array.SetValue(((IList)list)[i], i);
                    }
                    property.SetValue(instance, array, null);
                }
                else
                {
                    switch (valueType)
                    {
                        case VersionValueTypeEnum.BYTE:
                            property.SetValue(instance, data[index]);
                            index++;
                            break;
                        case VersionValueTypeEnum.SBYTE:
                            property.SetValue(instance, (sbyte)data[index]);
                            index++;
                            break;
                        case VersionValueTypeEnum.BOOL:
                            property.SetValue(instance, ParserConverter.GetBool(data[index]));
                            index++;
                            break;
                        case VersionValueTypeEnum.CHAR:
                            property.SetValue(instance, ParserConverter.GetChar(data[index]));
                            index++;
                            break;
                        case VersionValueTypeEnum.SHORT:
                            property.SetValue(instance, ParserConverter.GetShort(ParserConverter.SubArray(data, index, 2)));
                            index += 2;
                            break;
                        case VersionValueTypeEnum.USHORT:
                            property.SetValue(instance, ParserConverter.GetUShort(ParserConverter.SubArray(data, index, 2)));
                            index += 2;
                            break;
                        case VersionValueTypeEnum.INT:
                            property.SetValue(instance, ParserConverter.GetInt(ParserConverter.SubArray(data, index, 4)));
                            index += 4;
                            break;
                        case VersionValueTypeEnum.UINT:
                            property.SetValue(instance, ParserConverter.GetUInt(ParserConverter.SubArray(data, index, 4)));
                            index += 4;
                            break;
                        case VersionValueTypeEnum.LONG:
                            property.SetValue(instance, ParserConverter.GetLong(ParserConverter.SubArray(data, index, 8)));
                            index += 8;
                            break;
                        case VersionValueTypeEnum.ULONG:
                            property.SetValue(instance, ParserConverter.GetULong(ParserConverter.SubArray(data, index, 8)));
                            index += 8;
                            break;
                        case VersionValueTypeEnum.DECIMAL:
                            property.SetValue(instance, ParserConverter.GetDecimal(ParserConverter.SubArray(data, index, 16)));
                            index += 16;
                            break;
                        case VersionValueTypeEnum.FLOAT:
                            property.SetValue(instance, ParserConverter.GetFloat(ParserConverter.SubArray(data, index, 4)));
                            index += 4;
                            break;
                        case VersionValueTypeEnum.DOUBLE:
                            property.SetValue(instance, ParserConverter.GetDouble(ParserConverter.SubArray(data, index, 8)));
                            index += 8;
                            break;
                        case VersionValueTypeEnum.STRING:
                            int length = ParserConverter.GetInt(ParserConverter.SubArray(data, index, 4));
                            index += 4;

                            property.SetValue(instance, ParserConverter.GetString(ParserConverter.SubArray(data, index, length)));
                            index += length;
                            break;
                        case VersionValueTypeEnum.DATETIME:
                            property.SetValue(instance, ParserConverter.GetDateTime(ParserConverter.SubArray(data, index, 8)));
                            index += 8;
                            break;
                        case VersionValueTypeEnum.OBJECT:
                            bool hasData = ParserConverter.GetBool(data[index]);
                            index++;

                            if (hasData)
                            {
                                if (property.GetValue(instance, null) != null)
                                {
                                    property.SetValue(instance, GetObject(property.GetValue(instance, null), data, ref index));
                                }
                                else
                                {
                                    property.SetValue(instance, GetObject(Activator.CreateInstance(property.PropertyType), data, ref index));
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                switch (valueType)
                {
                    case VersionValueTypeEnum.BYTE:
                        returnVal = data[index];
                        index++;
                        break;
                    case VersionValueTypeEnum.SBYTE:
                        returnVal = (sbyte)data[index];
                        index++;
                        break;
                    case VersionValueTypeEnum.BOOL:
                        returnVal = ParserConverter.GetBool(data[index]);
                        index++;
                        break;
                    case VersionValueTypeEnum.CHAR:
                        returnVal = ParserConverter.GetChar(data[index]);
                        index++;
                        break;
                    case VersionValueTypeEnum.SHORT:
                        returnVal = ParserConverter.GetShort(ParserConverter.SubArray(data, index, 2));
                        index += 2;
                        break;
                    case VersionValueTypeEnum.USHORT:
                        returnVal = ParserConverter.GetUShort(ParserConverter.SubArray(data, index, 2));
                        index += 2;
                        break;
                    case VersionValueTypeEnum.INT:
                        returnVal = ParserConverter.GetInt(ParserConverter.SubArray(data, index, 4));
                        index += 4;
                        break;
                    case VersionValueTypeEnum.UINT:
                        returnVal = ParserConverter.GetUInt(ParserConverter.SubArray(data, index, 4));
                        index += 4;
                        break;
                    case VersionValueTypeEnum.LONG:
                        returnVal = ParserConverter.GetLong(ParserConverter.SubArray(data, index, 8));
                        index += 8;
                        break;
                    case VersionValueTypeEnum.ULONG:
                        returnVal = ParserConverter.GetULong(ParserConverter.SubArray(data, index, 8));
                        index += 8;
                        break;
                    case VersionValueTypeEnum.DECIMAL:
                        returnVal = ParserConverter.GetDecimal(ParserConverter.SubArray(data, index, 16));
                        index += 16;
                        break;
                    case VersionValueTypeEnum.FLOAT:
                        returnVal = ParserConverter.GetFloat(ParserConverter.SubArray(data, index, 4));
                        index += 4;
                        break;
                    case VersionValueTypeEnum.DOUBLE:
                        returnVal = ParserConverter.GetDouble(ParserConverter.SubArray(data, index, 8));
                        index += 8;
                        break;
                    case VersionValueTypeEnum.STRING:
                        int length = ParserConverter.GetInt(ParserConverter.SubArray(data, index, 4));
                        index += 4;

                        returnVal = ParserConverter.GetString(ParserConverter.SubArray(data, index, length));
                        index += length;
                        break;
                    case VersionValueTypeEnum.DATETIME:
                        returnVal = ParserConverter.GetDateTime(ParserConverter.SubArray(data, index, 8));
                        index += 8;
                        break;
                    case VersionValueTypeEnum.OBJECT:
                        bool hasData = ParserConverter.GetBool(data[index]);
                        index++;

                        if (hasData)
                        {
                            returnVal = GetObject(instance, data, ref index);
                        }
                        break;
                    default:
                        break;
                }
            }

            return returnVal;
        }
        #endregion

        #region Different Versions
        private static VersionData? VersionData;

        /// <summary>
        /// Indicates the saved data is older than the current class types, it parse with version data
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public static object GetVersionedObject(object instance, byte[] data, ref int index, ushort version)
        {
            //Get all versions
            VersionData = VersionDataController.GetVersionData(version);

            if (VersionData == null)
                throw new NullReferenceException($"The {version} number doesnt exists in version list.");


            Type type = instance.GetType();
            var properties = type.GetProperties();

            VersionInformationData? versionInformation = VersionData.VersionClasses.FirstOrDefault(s => s.ClassFullName == type.FullName && s.Name == type.Name);

            if (versionInformation == null)
            {
                //Add log or throw error based on your needs.
                return instance;
            }

            foreach (var subVersionInfo in versionInformation.Properties)
            {
                PropertyInfo? property = properties.FirstOrDefault(s => s.Name == subVersionInfo.PropertyName && s.PropertyType.FullName == subVersionInfo.PropertyClassFullName);

                if (property is null)
                {
                    PropertyInfo? primitiveProp = properties.FirstOrDefault(s => s.Name == subVersionInfo.PropertyName && s.PropertyType.IsPrimitive);

                    TryParseTypeChangedProperty(instance, ref index, primitiveProp, subVersionInfo, data, version);
                }
                else
                {
                    SetObjectPropertyFromVersion(instance, data, ref index, property, subVersionInfo, true);
                }
            }

            return instance;
        }

        public static object? SetObjectPropertyFromVersion(object instance, byte[] data, ref int index, PropertyInfo property, SubVersionInformationData subVersionInformation, bool fromProperty)
        {
            object? returnVal = null;

            if (fromProperty)
            {
                if (subVersionInformation.IsList)
                {
                    var listType = typeof(List<>);
                    Type listElementType = property.PropertyType.GetGenericArguments().Single();
                    var constructedList = listType.MakeGenericType(listElementType);
                    object list = Activator.CreateInstance(constructedList);

                    int length = GetArrayOrListLength(ref index, data);
                    IList? propVal = (IList?)property.GetValue(instance, null);

                    for (int i = 0; i < length; i++)
                    {
                        object? val = null;
                        if (propVal != null && i < propVal.Count && propVal[i] != null)
                            val = propVal[i];
                        else
                            val = Activator.CreateInstance(listElementType);

                        ((IList)list).Add(val);
                        var obj = SetObjectPropertyFromVersion(((IList)list)[i], data, ref index, property, subVersionInformation, fromProperty);
                        if(obj != null)
                            ((IList)list)[i] = obj;
                    }
                    property.SetValue(instance, list);
                }
                else if(subVersionInformation.IsArray) 
                {
                    var listType = typeof(List<>);
                    Type listElementType = property.PropertyType.GetGenericArguments().Single();
                    var constructedList = listType.MakeGenericType(listElementType);
                    object list = Activator.CreateInstance(constructedList);

                    int length = GetArrayOrListLength(ref index, data);
                    IList? propVal = (IList?)property.GetValue(instance, null);

                    for (int i = 0; i < length; i++)
                    {
                        object? val = null;
                        if (propVal != null && i < propVal.Count && propVal[i] != null)
                            val = propVal[i];
                        else
                            val = Activator.CreateInstance(listElementType);

                        ((IList)list).Add(val);
                        var obj = SetObjectPropertyFromVersion(((IList)list)[i], data, ref index, property, subVersionInformation, fromProperty);
                        if (obj != null)
                            ((IList)list)[i] = obj;
                    }

                    Array array = Array.CreateInstance(listType, length);
                    for (int i = 0; i < length; i++ )
                        array.SetValue(((IList)list)[i], i);
                    property.SetValue(instance, array);
                }
                else
                {

                }
            }
            else
            {

            }

            return returnVal;
        }

        #region Parsing Non Existing Property
        /// <summary>
        /// If a primitive type changes its type to another primitive type, the data can be recovered even if the data cannot be fully recovered.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="index"></param>
        /// <param name="primitiveProp"></param>
        /// <param name="subVersionInformation"></param>
        /// <param name="data"></param>
        /// <param name="version"></param>
        private static void TryParseTypeChangedProperty(object instance, ref int index, PropertyInfo? primitiveProp, SubVersionInformationData subVersionInformation, byte[] data, ushort version)
        {
            if (primitiveProp != null && IsPrimitiveValue(subVersionInformation.ValueType))
            {
                dynamic? val = ParsePrimitive(ref index, subVersionInformation.ValueType, data); 

                if(val != null)
                    primitiveProp.SetValue(instance, Convert.ChangeType(val, primitiveProp.PropertyType));
                else
                    primitiveProp.SetValue(instance, Activator.CreateInstance(primitiveProp.PropertyType));
            }
            else
            {
                JumpVersionClass(ref index, subVersionInformation, data);
            }
        }

        private static dynamic? ParsePrimitive(ref int index, VersionValueTypeEnum versionValue, byte[] data)
        {
            dynamic? returnVal = null;

            switch (versionValue)
            {
                case VersionValueTypeEnum.BYTE:
                    returnVal = data[index];
                    index++;
                    break;
                case VersionValueTypeEnum.SBYTE:
                    returnVal = (sbyte)data[index];
                    index++;
                    break;
                case VersionValueTypeEnum.BOOL:
                    returnVal = ParserConverter.GetBool(data[index]);
                    index++;
                    break;
                case VersionValueTypeEnum.CHAR:
                    returnVal = ParserConverter.GetChar(data[index]);
                    index++;
                    break;
                case VersionValueTypeEnum.SHORT:
                    returnVal = ParserConverter.GetShort(ParserConverter.SubArray(data, index, 2));
                    index += 2;
                    break;
                case VersionValueTypeEnum.USHORT:
                    returnVal = ParserConverter.GetUShort(ParserConverter.SubArray(data, index, 2));
                    index += 2;
                    break;
                case VersionValueTypeEnum.INT:
                    returnVal = ParserConverter.GetInt(ParserConverter.SubArray(data, index, 4));
                    index += 4;
                    break;
                case VersionValueTypeEnum.UINT:
                    returnVal = ParserConverter.GetUInt(ParserConverter.SubArray(data, index, 4));
                    index += 4;
                    break;
                case VersionValueTypeEnum.LONG:
                    returnVal = ParserConverter.GetLong(ParserConverter.SubArray(data, index, 8));
                    index += 8;
                    break;
                case VersionValueTypeEnum.ULONG:
                    returnVal = ParserConverter.GetULong(ParserConverter.SubArray(data, index, 8));
                    index += 8;
                    break;
                case VersionValueTypeEnum.DECIMAL:
                    returnVal = ParserConverter.GetDecimal(ParserConverter.SubArray(data, index, 16));
                    index += 16;
                    break;
                case VersionValueTypeEnum.FLOAT:
                    returnVal = ParserConverter.GetFloat(ParserConverter.SubArray(data, index, 4));
                    index += 4;
                    break;
                case VersionValueTypeEnum.DOUBLE:
                    returnVal = ParserConverter.GetDouble(ParserConverter.SubArray(data, index, 8));
                    index += 8;
                    break;
                case VersionValueTypeEnum.STRING:
                    int length = ParserConverter.GetInt(ParserConverter.SubArray(data, index, 4));
                    index += 4;

                    returnVal = ParserConverter.GetString(ParserConverter.SubArray(data, index, length));
                    index += length;
                    break;
                case VersionValueTypeEnum.DATETIME:
                    returnVal = ParserConverter.GetDateTime(ParserConverter.SubArray(data, index, 8));
                    index += 8;
                    break;
                default:
                    break;
            }

            return returnVal;
        }

        private static void JumpVersionClass(ref int index, SubVersionInformationData subVersionInformation, byte[] data)
        {
            if (subVersionInformation.IsList || subVersionInformation.IsArray)
            {
                int len = GetArrayOrListLength(ref index, data);
                for (int i = 0; i < len; i++)
                {
                    JumpIndex(ref index, subVersionInformation, data);
                }
            }
            else
            {
                JumpIndex(ref index, subVersionInformation, data);
            }
        }

        private static void JumpIndex(ref int index, SubVersionInformationData subVersionInformation, byte[] data)
        {
            switch (subVersionInformation.ValueType)
            {
                case VersionValueTypeEnum.BYTE:
                    index++;
                    break;
                case VersionValueTypeEnum.SBYTE:
                    index++;
                    break;
                case VersionValueTypeEnum.BOOL:
                    index++;
                    break;
                case VersionValueTypeEnum.CHAR:
                    index++;
                    break;
                case VersionValueTypeEnum.SHORT:
                    index += 2;
                    break;
                case VersionValueTypeEnum.USHORT:
                    index += 2;
                    break;
                case VersionValueTypeEnum.INT:
                    index += 4;
                    break;
                case VersionValueTypeEnum.UINT:
                    index += 4;
                    break;
                case VersionValueTypeEnum.LONG:
                    index += 8;
                    break;
                case VersionValueTypeEnum.ULONG:
                    index += 8;
                    break;
                case VersionValueTypeEnum.DECIMAL:
                    index += (4 * 4);
                    break;
                case VersionValueTypeEnum.FLOAT:
                    index += 4;
                    break;
                case VersionValueTypeEnum.DOUBLE:
                    index += 8;
                    break;
                case VersionValueTypeEnum.STRING:
                    int strLen = ParserConverter.GetInt(ParserConverter.SubArray(data, index, 4));
                    index += 4;

                    ParserConverter.GetString(ParserConverter.SubArray(data, index, strLen));
                    index += strLen;
                    break;
                case VersionValueTypeEnum.DATETIME:
                    index += 8;
                    break;
                case VersionValueTypeEnum.OBJECT:
                    bool hasData = ParserConverter.GetBool(data[index]);
                    index++;
                    //The object has data control all its versioned props
                    if (hasData)
                    {
                        //The version is not null here since in our entry point (GetVersionedObject()) checks the varaible.
                        var versionedClass = VersionData.VersionClasses.FirstOrDefault(s => s.ClassFullName == subVersionInformation.ClassFullName);

                        if (versionedClass != null)
                            foreach (var subVer in versionedClass.Properties)
                                JumpVersionClass(ref index, subVer, data);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Gets if versioned value is primitive or not
        /// </summary>
        /// <param name="versionValue"></param>
        /// <returns>Returns true if versioned value is primitive otherwise false</returns>
        private static bool IsPrimitiveValue(VersionValueTypeEnum versionValue)
        {
            switch (versionValue)
            {
                case VersionValueTypeEnum.BYTE:
                case VersionValueTypeEnum.SBYTE:
                case VersionValueTypeEnum.BOOL:
                case VersionValueTypeEnum.CHAR:
                case VersionValueTypeEnum.SHORT:
                case VersionValueTypeEnum.USHORT:
                case VersionValueTypeEnum.INT:
                case VersionValueTypeEnum.UINT:
                case VersionValueTypeEnum.LONG:
                case VersionValueTypeEnum.ULONG:
                case VersionValueTypeEnum.DECIMAL:
                case VersionValueTypeEnum.FLOAT:
                case VersionValueTypeEnum.DOUBLE:
                    return true;
                case VersionValueTypeEnum.STRING:
                case VersionValueTypeEnum.DATETIME:
                case VersionValueTypeEnum.OBJECT:
                default:
                    return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="data"></param>
        /// <returns>Return an array or list length from data</returns>
        private static int GetArrayOrListLength(ref int index, byte[] data)
        {
            int length = ParserConverter.GetInt(ParserConverter.SubArray(data, index, 4));
            index += 4;

            return length;
        }
        #endregion
        #endregion
    }
}
