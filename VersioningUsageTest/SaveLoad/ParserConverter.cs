using System.Collections;
using System.Text;

namespace VersioningUsageTest.SaveLoad
{
    public static class ParserConverter
    {
        public static T[] SubArray<T>(T[] data, int index, int count)
        {
            T[] result = new T[count];
            Array.Copy(data, index, result, 0, count);
            return result;
        }

        #region GetBytesFromValue
        public static byte[] GetBytes(bool value)
        {
            return [(byte)(value ? 1 : 0)];
        }

        public static byte[] GetBytes(char value)
        {
            return [(byte)value];
        }

        public static byte[] GetBytes(short value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] GetBytes(ushort value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] GetBytes(int value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] GetBytes(uint value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] GetBytes(long value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] GetBytes(ulong value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] GetBytes(decimal value)
        {
            int[] bits = decimal.GetBits(value);

            // Create a byte array to hold the 16 bytes (4 integers * 4 bytes each).
            byte[] bytes = new byte[16];

            // Copy the bytes of each integer into the byte array.
            for (int i = 0; i < bits.Length; i++)
            {
                byte[] tempBytes = BitConverter.GetBytes(bits[i]);
                Array.Copy(tempBytes, 0, bytes, i * 4, 4);
            }

            return bytes;
        }

        public static byte[] GetBytes(float value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] GetBytes(double value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] GetBytes(string value)
        {
            if (value == null)
                return new byte[0];

            List<byte> bytes = Encoding.UTF8.GetBytes(value).OfType<byte>().ToList();
            bytes.InsertRange(0, GetBytes(bytes.Count));

            return bytes.ToArray();
        }

        public static byte[] GetBytes(DateTime value)
        {
            return BitConverter.GetBytes(value.Ticks);
        }
        #endregion

        #region GetValueFromByteArray
        public static bool GetBool(byte data)
        {
            return data != 0;
        }

        public static char GetChar(byte data)
        {
            return (char)data;
        }

        public static short GetShort(byte[] data)
        {
            return BitConverter.ToInt16(data);
        }

        public static ushort GetUShort(byte[] data)
        {
            return BitConverter.ToUInt16(data);
        }

        public static int GetInt(byte[] data)
        {
            return BitConverter.ToInt32(data);
        }

        public static uint GetUInt(byte[] data)
        {
            return BitConverter.ToUInt32(data); 
        }

        public static long GetLong(byte[] data)
        {
            return BitConverter.ToInt64(data);
        }

        public static ulong GetULong(byte[] data)
        {
            return BitConverter.ToUInt64(data);
        }

        public static decimal GetDecimal(byte[] data)
        {
            int[] bits = new int[4];

            for (int i = 0; i < bits.Length; i++)
            {
                bits[i] = BitConverter.ToInt32(data, i * 4);
            }

            return new decimal(bits);
        }

        public static float GetFloat(byte[] data)
        {
            return BitConverter.ToSingle(data);
        }

        public static double GetDouble(byte[] data)
        {
            return BitConverter.ToDouble(data);
        }

        public static string GetString(byte[] data)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            byte[] stringBytes = new byte[data.Length];
            Array.Copy(data, 4, stringBytes, 0, data.Length);

            return Encoding.UTF8.GetString(stringBytes);
        }

        public static DateTime GetDateTime(byte[] data)
        {
            return new DateTime(GetLong(data));
        }
        #endregion
    }
}
