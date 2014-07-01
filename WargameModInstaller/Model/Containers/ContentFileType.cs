using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model.Containers
{
    //To do: pmyśleć o zdefiniowaniu w bazie Enumeration listy zadeklarownyxh typów z klas potomych,
    //ktróe były by przekazywane w konstruktorze z kazdej kalsy potomnej i dodawane do juz istniejacyhc z kalsy bazowej (łaczenie list).
    //To umożliwiło by ujednoslicenie metod zbiorczych silnie typowanych typu GetAll, bo obecne meotdy klasy bazowej Enum maja maknakemnty:
    //wymaganie publicnzego kontrukotr, nie uwzgledinianie typów pochodnych itd.

    public class ContentFileType : Enumeration
    {
        public static readonly ContentFileType Ndfbin = 
            new ContentFileType(1, "Ndfbin", new byte[] { 0x45, 0x55, 0x47, 0x30, 0x00, 0x00, 0x00, 0x00, 0x43, 0x4E, 0x44, 0x46 });
        public static readonly ContentFileType Trad =
            new ContentFileType(2, "Trad", new byte[] { 0x54, 0x52, 0x41, 0x44 });
        public static readonly ContentFileType Image = 
            new ContentFileType(4, "Image", new byte[] { 0x02 });
        public static readonly ContentFileType Edata =
            new ContentFileType(5, "Edata", new byte[] { 0x65, 0x64, 0x61, 0x74 });
        public static readonly ContentFileType Save = 
            new ContentFileType(6, "Save", new byte[] { 0x53, 0x41, 0x56, 0x30, 0x00, 0x00, 0x00, 0x00 });
        public static readonly ContentFileType Prxypcpc = 
            new ContentFileType(7, "Prxypcpc", new byte[] { 0x50, 0x52, 0x58, 0x59, 0x50, 0x43, 0x50, 0x43 });
        public static readonly ContentFileType Unknown = 
            new ContentFileType(8, "Unknown", new byte[] {});

        protected static HashSet<ContentFileType> knownTypes;

        static ContentFileType()
        {
            knownTypes = new HashSet<ContentFileType>();
            knownTypes.Add(Ndfbin);
            knownTypes.Add(Trad);
            knownTypes.Add(Image);
            knownTypes.Add(Edata);
            knownTypes.Add(Save);
            knownTypes.Add(Prxypcpc);
            knownTypes.Add(Unknown);
        }

        public static IEnumerable<ContentFileType> GetAll()
        {
            return knownTypes;
        }

        protected ContentFileType(int value, String name, byte[] magic)
            : base(value, name)
        {
            this.MagicBytes = magic;
        }

        public byte[] MagicBytes
        {
            get;
            protected set;
        }
    }
}
