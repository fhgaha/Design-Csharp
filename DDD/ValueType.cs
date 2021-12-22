using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ddd.Infrastructure
{
    /// <summary>
    /// Базовый класс для всех Value типов.
    /// </summary>
    public class ValueType<T>
    {
        static PropertyInfo[] props;

        static ValueType()
        {
            props = typeof(T).GetProperties();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((T)obj);
        }

        public bool Equals(T obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;

            for (int i = 0; i < props.Length; i++)
            {
                var value = props[i].GetValue(this);
                var valueObj = props[i].GetValue(obj);
                if (value == null && valueObj == null) continue;
                if (!value.Equals(valueObj)) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            var values = props.Select(p => p.GetValue(this)).ToArray();

            unchecked
            {
                int hash = 123;
                int p = 111;
                for (int i = 0; i < values.Length; i++)
                {
                    hash *= p;
                    hash ^= values[i].GetHashCode();
                }
                return hash;
            }
        }

        public override string ToString()
        {
            var props = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var propsNames = props.Select(p => p.Name).ToArray();
            var propsValues = props.Select(p => p.GetValue(this)).ToArray();
            var propsPairs = new List<string>();

            for (int i = 0; i < props.Length; i++)
                propsPairs.Add($"{propsNames[i]}: {propsValues[i]}");

            propsPairs.Sort();
            var propsJoined = string.Join("; ", propsPairs);
            return $"{GetType().Name}(" + propsJoined + ")";
        }
    }
}
