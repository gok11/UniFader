using System;
using System.Collections.Generic;
using System.Linq;

namespace MB.UniFader
{
    public class TypeUtility
    {
        public static Dictionary<string, Type> GetTypeDictWithCustomDrawerAttribute()
        {
            var drawerDict = new Dictionary<string, Type>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.GetCustomAttributes(typeof(CustomSerializeReferenceDrawer), false).Length <= 0)
                        continue;

                    var drawer = (CustomSerializeReferenceDrawer) Attribute.GetCustomAttribute(type, typeof(CustomSerializeReferenceDrawer));
                    drawerDict.Add(drawer.type.Name, type);
                }
            }

            return drawerDict;
        }
    
        // Type.GetType doesn't work on Unity
        public static Type GetTypeByName(string className)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type => type.Name == className);
        }
    
        public static string[] GetAllFadePatternNames<T>() where T : class
        {
            var allFadePatternNames = new List<string>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var types = assembly.GetTypes()
                    .Where(t => t != typeof(T))
                    .Where(t => typeof(T).IsAssignableFrom(t))
                    .Select(t => t.Name);
                allFadePatternNames.AddRange(types);
            }

            return allFadePatternNames.ToArray();
        }
    }
}
