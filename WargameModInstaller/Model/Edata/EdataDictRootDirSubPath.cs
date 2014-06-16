using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Edata
{
    public class EdataDictRootDirSubPath : EdataDictDirSubPath
    {
        public EdataDictRootDirSubPath(String subPath)
            : base(subPath)
        {
           
        }

        protected override uint GetRelevanceLength()
        {
            return 0;
        }

    }
}
