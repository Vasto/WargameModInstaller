using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WargameModInstaller.Services.Utilities;

namespace WargameModInstaller.Common.Extensions
{
    public static class DialogExtensions
    {
        public static bool ToBool(this DialogResult obj)
        {
            switch (obj)
            {
                case DialogResult.OK:
                case DialogResult.Yes:
                    return true;

                case DialogResult.No:
                case DialogResult.Abort:
                case DialogResult.None:
                case DialogResult.Cancel:
                    return false;

                default:
                    throw new ApplicationException("Unexpected DialogResult");
            }
        }

        public static bool? ToBoolOrNull(this DialogResult obj)
        {
            switch (obj)
            {
                case DialogResult.OK:
                case DialogResult.Yes:
                    return true;

                case DialogResult.No:
                case DialogResult.Abort:
                    return false;

                case DialogResult.None:
                case DialogResult.Cancel:
                    return null;

                default:
                    throw new ApplicationException("Unexpected DialogResult");
            }
        }

        public static bool ToBool(this MessageResult obj)
        {
            switch (obj)
            {
                case MessageResult.OK:
                case MessageResult.Yes:
                    return true;

                case MessageResult.No:
                case MessageResult.Cancel:
                case MessageResult.None:
                    return false;

                default:
                    throw new ApplicationException("Unexpected MessageResult");
            }
        }

        public static bool? ToBoolOrNull(this MessageResult obj)
        {
            switch (obj)
            {
                case MessageResult.OK:
                case MessageResult.Yes:
                    return true;

                case MessageResult.No:
                case MessageResult.Cancel:
                    return false;

                case MessageResult.None:
                    return null;

                default:
                    throw new ApplicationException("Unexpected MessageResult");
            }
        }

    }
}
