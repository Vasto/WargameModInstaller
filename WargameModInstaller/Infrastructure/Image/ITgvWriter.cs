using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WargameModInstaller.Model.Image;

namespace WargameModInstaller.Infrastructure.Image
{
    /// <summary>
    /// Interfejs zapisu pliku tgv.
    /// </summary>
    public interface ITgvWriter
    {
        void Write(TgvImage file);
    }
}
