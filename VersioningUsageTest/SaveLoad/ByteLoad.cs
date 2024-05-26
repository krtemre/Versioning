using System.Collections;
using System.IO;
using System.Reflection;
using Versioning;

namespace VersioningUsageTest.SaveLoad
{
    public static class ByteLoad
    {
        public static object LoadObject(object obj, string path)
        {
            if (!File.Exists(path)) { return new object(); }

            FileStream fileStream = new FileStream(path, FileMode.Open);
            BinaryReader reader = new BinaryReader(fileStream);

            short saveObjVersion = reader.ReadInt16();

            int length = reader.ReadInt32();

            byte[] data = reader.ReadBytes(length);

            return ParseObject(obj, data, saveObjVersion);
        }

        public static object ParseObject(object instance, byte[] data, short version)
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

                    int length = ParserConverter.GetInt(ParserConverter.SubArray(data, index, 4));
                    index += 4;

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

                    int length = ParserConverter.GetInt(ParserConverter.SubArray(data, index, 4));
                    index += 4;

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
                                if(property.GetValue(instance, null) != null)
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
        public static object GetVersionedObject(object instance, byte[] data, ref int index, short version)
        {
            return instance;
        }
        #endregion
    }
}
