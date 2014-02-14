using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Common.Extensions
{
    public static class WMIPathExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceObj"></param>
        /// <param name="pathToAppend"></param>
        /// <returns></returns>
        public static String GetAbsoluteOrAppendIfRelative(this WMIPath sourceObj, String pathToAppend)
        {
            if (sourceObj.PathType == WMIPathType.Absolute)
            {
                return sourceObj.Value;
            }
            else if (sourceObj.PathType == WMIPathType.Relative)
            {
                return Path.Combine(sourceObj.Value, pathToAppend);
            }
            else
            {
                throw new ArgumentException("The given path is not absolute or relative", "path");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceObj"></param>
        /// <param name="pathToPrepend"></param>
        /// <returns></returns>
        public static String GetAbsoluteOrPrependIfRelative(this WMIPath sourceObj, String pathToPrepend)
        {
            if (sourceObj.PathType == WMIPathType.Absolute)
            {
                return sourceObj.Value;
            }
            else if (sourceObj.PathType == WMIPathType.Relative)
            {
                return Path.Combine(pathToPrepend, sourceObj.Value);
            }
            else
            {
                throw new ArgumentException("The given path is not absolute or relative", "path");
            }
        }

    }
}
