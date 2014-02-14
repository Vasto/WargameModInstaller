using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WargameModInstaller.Common.Extensions
{
    public static class CancellationTokenExtensions
    {
        public static void ThrowIfCanceledAndNotNull(this Nullable<CancellationToken> obj)
        {
            if (obj.HasValue)
            {
                obj.Value.ThrowIfCancellationRequested();
            }
        }

    }

}
