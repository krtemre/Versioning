using System.Collections;
using System.Reflection;

namespace VersioningUsageTest.SaveLoad
{
    public static class Utils
    {
        private static Dictionary<Type, bool> listObjectCache = new Dictionary<Type, bool>();

        public static bool IsList(object obj)
        {
            if (obj == null) return false;

            Type t = obj.GetType();

            if (!listObjectCache.ContainsKey(t))
            {
                bool isList = obj is IList &&
                    t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>);

                listObjectCache.Add(t, isList);
            }

            return listObjectCache[t];
        }

        public static bool IsList(PropertyInfo property)
        {
            if (property == null) return false;

            if (!listObjectCache.ContainsKey(property.PropertyType))
            {
                bool isLİst = property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition().Equals(typeof(List<>));
                listObjectCache.Add(property.PropertyType, isLİst);
            }

            return listObjectCache[property.PropertyType];
        }
    }
}
