using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WargameModInstaller.Model.Image;

namespace WargameModInstaller.Infrastructure.Image
{
    public class TgvDDSWriter : ITgvWriter
    {
        public TgvDDSWriter(String ddsFilePath)
        {
            this.DDSFilePath = ddsFilePath;
        }

        protected String DDSFilePath
        {
            get;
            private set;
        }


        public virtual void Write(TgvImage file)
        {
            throw new NotImplementedException();
        }

    }
}
