using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace WargameModInstaller.Common.Extensions
{
    public static class XExtensions
    {
        /// <summary>
        /// Converts the specified XPath value to the given type if possible.
        /// If not, returns a default value for the type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T ToOrDefault<T>(this XPathNavigator obj)
        {
            if (obj != null && obj.TypedValue != null)
            {
                var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
                if (converter.IsValid(obj.TypedValue))
                {
                    return (T)converter.ConvertFrom(obj.TypedValue);
                }
            }

            return default(T);
        }

        public static T ValueOrDefault<T>(this XElement obj)
        {
            return obj.ValueOr<T>(default(T));
        }

        public static T ValueOrDefault<T>(this XAttribute obj)
        {
            return obj.ValueOr<T>(default(T));
        }

        public static T ValueOr<T>(this XElement obj, T defaultValue)
        {
            if (obj != null && !String.IsNullOrWhiteSpace(obj.Value))
            {
                var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
                if (converter.IsValid(obj.Value))
                {
                    return (T)converter.ConvertFrom(obj.Value);
                }
            }

            return defaultValue;
        }

        public static T ValueOr<T>(this XAttribute obj, T defaultValue)
        {
            if (obj != null && !String.IsNullOrWhiteSpace(obj.Value))
            {
                var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));
                if (converter.IsValid(obj.Value))
                {
                    return (T)converter.ConvertFrom(obj.Value);
                }
            }

            return defaultValue;
        }

        public static String ValueNullSafe(this XElement obj)
        {
            String result = null;
            if (obj != null)
            {
                result = obj.Value;
            }

            return result;
        }

        public static String ValueNullSafe(this XAttribute obj)
        {
            String result = null;
            if (obj != null)
            {
                result = obj.Value;
            }

            return result;
        }

    }

}
