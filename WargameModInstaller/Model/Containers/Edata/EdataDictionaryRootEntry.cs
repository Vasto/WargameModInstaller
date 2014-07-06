using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Containers.Edata
{
    public class EdataDictionaryRootEntry : EdataDictionaryDirEntry
    {
        public EdataDictionaryRootEntry()
            : base(String.Empty)
        {

        }

        public override EdataDictionaryPathEntry SelectEntryByPath(String path)
        {
            foreach (var followingEntry in FollowingEntries)
            {
                var result = followingEntry.SelectEntryByPath(path);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        public override byte[] ToBytes()
        {
            byte[] content = { 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            return content;
        }

        protected override int GetLengthInBytes()
        {
            return 10;
        }

        protected override int GetRelevanceLength()
        {
            return 0;
        }

    }
}
