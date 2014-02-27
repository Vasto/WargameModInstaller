using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WargameModInstaller.Model.Image;

namespace WargameModInstaller.Infrastructure.Image
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITgvFileWriter
    {
        void Write(TgvImage file, String filePath);
    }
}
