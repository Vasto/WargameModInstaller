using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Common.Utilities
{
    public static class MathUtilities
    {
        /// <summary>
        /// Restricts value to be within a specific boundary.
        /// </summary>
        /// <typeparam name="T">Typ wartości do przycięcia.</typeparam>
        /// <param name="value">Wartość.</param>
        /// <param name="min">Minimalna dopuszczalna wartość.</param>
        /// <param name="max">Maksymalna dopuszczalna wartość.</param>
        /// <returns>Wartośc przycięta do przedziału.</returns>
        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0) return min;
            else if (value.CompareTo(max) > 0) return max;
            else return value;
        }
    }
}
