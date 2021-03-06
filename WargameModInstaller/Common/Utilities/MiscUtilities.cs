using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Common.Utilities
{
    public static class MiscUtilities
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int memcmp(byte[] b1, byte[] b2, long count);

        public static bool ComparerByteArrays(byte[] b1, byte[] b2)
        {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  
            return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
        }

        /// <summary>
        /// Reads an null terminated string from stream.
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        public static string ReadString(Stream fs, bool swallowNullByte = true)
        {
            var b = new StringBuilder();
            var buffer = new byte[1];
            char c;

            do
            {
                fs.Read(buffer, 0, 1);
                c = Encoding.ASCII.GetChars(buffer)[0];
                b.Append(c);
            }
            while (c != '\0');

            if (!swallowNullByte)
            {
                fs.Seek(-1, SeekOrigin.Current);
            }

            return b.ToString().Split('\0')[0].TrimEnd();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        public static string GenerateCoupon(int length, Random random)
        {
            const string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var result = new StringBuilder(length);

            for (int i = 0; i < length; i++)
                result.Append(characters[random.Next(characters.Length)]);

            return result.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        public static byte[] CreateLocalisationHash(string text, int maxSize = 8)
        {
            long hash = 0;
            for (int i = 0; i < maxSize; ++i)
            {
                int value;
                ushort chr = text[i];

                if (chr == 0)
                    break;

                if ('0' <= chr && chr <= '9')
                    value = 1 + chr - '0';
                else if ('A' <= chr && chr <= 'Z')
                    value = 2 + '9' - '0' + chr - 'A';
                else if (chr == '_')
                    value = 3 + '9' - '0' + 'Z' - 'A';
                else if ('a' <= chr && chr <= 'z')
                    value = 4 + '9' - '0' + 'Z' - 'A' + chr - 'a';
                else
                    throw new InvalidDataException("");

                hash = (hash << 6) | value;
            }

            return BitConverter.GetBytes(hash);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            return stuff;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        public static byte[] StructToBytes(object str)
        {
            if (!IsValueType(str))
            {
                throw new ArgumentException("str");
            }

            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        public static bool IsValueType(object obj)
        {
            return obj != null && obj.GetType().IsValueType;
        }

        public static byte[] HexByteStringToByteArray(String hex)
        {
            int numberChars = hex.Length / 2;
            byte[] bytes = new byte[numberChars];
            using (var sr = new StringReader(hex))
            {
                for (int i = 0; i < numberChars; i++)
                {
                    bytes[i] = Convert.ToByte(new String(new char[2] { (char)sr.Read(), (char)sr.Read() }), 16);
                }
            }

            return bytes;
        }

        /// <summary>
        /// Returns the property name in a string form for the given property expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public static String GetPropertyName<T>(Expression<Func<T>> property)
        {
            var body = property.Body as MemberExpression;
            if (body == null)
            {
                body = ((UnaryExpression)property.Body).Operand as MemberExpression;
            }

            return body.Member.Name;
        }

        ///// <summary>
        ///// Returns the property name in a string form for the given property expression.
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="property"></param>
        ///// <returns></returns>
        //public static String GetPropertyName<T>(Expression<Func<T, object>> property)
        //{
        //    var body = property.Body as MemberExpression;
        //    if (body == null)
        //    {
        //        body = ((UnaryExpression)property.Body).Operand as MemberExpression;
        //    }

        //    return body.Member.Name;
        //}

        /// <summary>
        /// Returns the executing assembly version.
        /// </summary>
        /// <returns></returns>
        public static String GetAssemblyVerison()
        {
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            String versionString = "Version: " +
                version.Major.ToString() + "." +
                version.Minor.ToString() + "." +
                version.Build.ToString() + "." +
                version.Revision.ToString();

            return versionString;
        }

    }
}
