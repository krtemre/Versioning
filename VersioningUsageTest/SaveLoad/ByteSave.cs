using System.Collections;
using System.IO;
using Versioning;

namespace VersioningUsageTest.SaveLoad
{
    public static class ByteSave
    {
        public static void SaveObject(object obj, string fileLocation)
        {
            FileStream fileStream = new FileStream(fileLocation, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fileStream);

            bw.Write(VersionManager.VersionNumber);

            byte[] byteObj = GetByteArray(obj);

            bw.Write(byteObj.Length);
            bw.Write(byteObj);

            bw.Close();
            fileStream.Close();
        }

        public static byte[] GetByteArray(object obj)
        {
            Type type = obj.GetType();
            var attributes = (VersionClassAttribute[])type.GetCustomAttributes(typeof(VersionClassAttribute), false);

            //Control if it is a version Class
            if (attributes != null && attributes.Length > 0)
            {
                return GetByteArrayInternal(obj);
            }

            return new byte[0];
        }

        private static byte[] GetByteArrayInternal(object obj)
        {
            List<byte> list = new List<byte>();

            Type type = obj.GetType();

            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                var atts = (VersionPropertyAttribute[])property.GetCustomAttributes(typeof(VersionPropertyAttribute), false);

                if (atts != null && atts.Length > 0)
                {
                    var att = atts[0];

                    list.AddRange(GetByteArrayFromProperty(property.GetValue(obj), att.TypeEnum));
                }
            }

            return list.ToArray();
        }

        private static byte[] GetByteArrayFromProperty(object obj, VersionValueTypeEnum versionValue)
        {
            bool isList = Utils.IsList(obj);
            bool isArray = obj is Array;

            List<byte> list = new List<byte>();

            if (isList && obj != null)
            {
                IList? iList = obj as IList;

                if (iList != null)
                {
                    list.AddRange(ParserConverter.GetBytes(iList.Count));
                    for (int i = 0; i < iList.Count; i++)
                    {
                        if (obj != null)
                        {
                            list.AddRange(GetByteArrayFromProperty(iList[0], versionValue));
                        }
                    }
                }
            }
            else if (isArray && obj != null)
            {
                Array arr = obj as Array;

                if (arr != null)
                {
                    list.AddRange(ParserConverter.GetBytes(arr.Length));

                    for (int i = 0; i < arr.Length; i++)
                    {
                        list.AddRange(GetByteArrayFromProperty(arr.GetValue(i), versionValue));
                    }
                }
            }
            else
            {
                switch (versionValue)
                {
                    case VersionValueTypeEnum.BYTE:
                        list.Add((byte)obj);
                        break;
                    case VersionValueTypeEnum.SBYTE:
                        list.Add((byte)(sbyte)obj);
                        break;
                    case VersionValueTypeEnum.BOOL:
                        list.AddRange(ParserConverter.GetBytes((bool)obj));
                        break;
                    case VersionValueTypeEnum.CHAR:
                        list.AddRange(ParserConverter.GetBytes((char)obj));
                        break;
                    case VersionValueTypeEnum.SHORT:
                        list.AddRange(ParserConverter.GetBytes((sbyte)obj));
                        break;
                    case VersionValueTypeEnum.USHORT:
                        list.AddRange(ParserConverter.GetBytes((ushort)obj));
                        break;
                    case VersionValueTypeEnum.INT:
                        list.AddRange(ParserConverter.GetBytes((int)obj));
                        break;
                    case VersionValueTypeEnum.UINT:
                        list.AddRange(ParserConverter.GetBytes((uint)obj));
                        break;
                    case VersionValueTypeEnum.LONG:
                        list.AddRange(ParserConverter.GetBytes((long)obj));
                        break;
                    case VersionValueTypeEnum.ULONG:
                        list.AddRange(ParserConverter.GetBytes((ulong)obj));
                        break;
                    case VersionValueTypeEnum.DECIMAL:
                        list.AddRange(ParserConverter.GetBytes((decimal)obj));
                        break;
                    case VersionValueTypeEnum.FLOAT:
                        list.AddRange(ParserConverter.GetBytes((float)obj));
                        break;
                    case VersionValueTypeEnum.DOUBLE:
                        list.AddRange(ParserConverter.GetBytes((double)obj));
                        break;
                    case VersionValueTypeEnum.STRING:
                        list.AddRange(ParserConverter.GetBytes((string)obj));
                        break;
                    case VersionValueTypeEnum.DATETIME:
                        list.AddRange(ParserConverter.GetBytes((DateTime)obj));
                        break;
                    case VersionValueTypeEnum.OBJECT:
                        //if obj has value add true boolean to list else false
                        if (obj != null)
                        {
                            list.AddRange(ParserConverter.GetBytes(true));
                            list.AddRange(GetByteArrayInternal(obj));
                        }
                        else
                        {
                            list.AddRange(ParserConverter.GetBytes(false));
                        }
                        break;
                    default:
                        break;
                }
            }

            return list.ToArray();
        }
    }
}
