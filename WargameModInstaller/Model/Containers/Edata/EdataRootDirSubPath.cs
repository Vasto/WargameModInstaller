using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Containers.Edata
{
    public class EdataRootDirSubPath : EdataDirSubPath
    {
        public EdataRootDirSubPath(String subPath)
            : base(subPath)
        {
           
        }

        public override byte[] ToBytes()
        {
            byte[] buffer;
            using (var ms = new MemoryStream())
            {
                buffer = BitConverter.GetBytes(Length);
                ms.Write(buffer, 0, buffer.Length);

                //Relevance
                buffer = BitConverter.GetBytes(0u);
                ms.Write(buffer, 0, buffer.Length);

                buffer = Encoding.ASCII.GetBytes(SubPath);
                ms.Write(buffer, 0, buffer.Length);

                buffer = new byte[1];
                ms.Write(buffer, 0, buffer.Length);

                //Supplement to evenness
                if (ms.Length % 2 != 0)
                {
                    buffer = new byte[1];
                    ms.Write(buffer, 0, buffer.Length);
                }

                return ms.ToArray();
            }
        }

    }
}
