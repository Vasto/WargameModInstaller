using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Common.Utilities
{
    public class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[] left, byte[] right)
        {
            if (left == null || right == null)
            {
                return left == right;
            }

            return MiscUtilities.ComparerByteArrays(left, right);
        }

        public int GetHashCode(byte[] key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            int sum = 0;
            for (int i = 0; i < key.Length; ++i)
            {
                sum += key[i];
            }

            return sum;
        }
    }
}
