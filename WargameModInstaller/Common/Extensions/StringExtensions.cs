using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Common.Extensions
{
    public static class StringExtensions
    {
        public static T ToOrDefault<T>(this String obj)
        {
            return obj.ToOr(default(T));
        }

        public static T ToOr<T>(this String obj, T defaultValue)
        {
            if (!String.IsNullOrWhiteSpace(obj))
            {
                var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
                if (converter.IsValid(obj))
                {
                    return (T)converter.ConvertFrom(obj);
                }
            }

            return defaultValue;
        }

    }

}
