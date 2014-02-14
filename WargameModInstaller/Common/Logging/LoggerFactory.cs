using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Common.Logging
{
    public static class LoggerFactory
    {
        public static ILog Create(Type type)
        {
            return new Logger(type);
        }
    }
}
