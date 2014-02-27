using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Common.Entities
{
    public abstract class Enumeration : IComparable, IEquatable<Enumeration>
    {
        protected Enumeration(int value, String name)
        {
            this.Value = value;
            this.Name = name;
        }

        /// <summary>
        /// Gets the enumeration number value.
        /// </summary>
        public int Value
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the enumaration name.
        /// </summary>
        public String Name
        {
            get;
            private set;
        }

        public virtual bool Equals(Enumeration other)
        {
            Enumeration enumeration = other as Enumeration;
            if (enumeration != null)
            {
                return (enumeration.Value == this.Value) &&
                    (enumeration.Name == this.Name);
            }
            else
            {
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            Enumeration other = obj as Enumeration;
            if (other != null)
            {
                return Equals(other);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode() + Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(object obj)
        {
            return Value.CompareTo(((Enumeration)obj).Value);
        }

        public static IEnumerable<T> GetAll<T>() where T : Enumeration, new()
        {
            var type = typeof(T);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            foreach (var info in fields)
            {
                var instance = new T();
                var locatedValue = info.GetValue(instance) as T;

                if (locatedValue != null)
                {
                    yield return locatedValue;
                }
            }
        }

        public static IEnumerable<Enumeration> GetAll(Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            foreach (var info in fields)
            {
                var instance = Activator.CreateInstance(type);
                yield return (Enumeration)info.GetValue(instance);
            }
        }

    }

}
