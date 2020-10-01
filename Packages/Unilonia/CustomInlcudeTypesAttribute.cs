using System;
using System.Collections.Generic;
using Avalonia;
using TypeReferences;

namespace Unilonia
{
    [AttributeUsage(AttributeTargets.Field)]
    public class CustomInlcudeTypesAttribute : TypeOptionsAttribute
    {
        public CustomInlcudeTypesAttribute()
        {
            Grouping = Grouping.None;
            var result = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(typeof(Application)))
                        result.Add(type);
                }
            }
            IncludeTypes = result.ToArray();
        }

        public override bool MatchesRequirements(Type type)
        {
            return type.IsSubclassOf(typeof(Application));
        }
    }
}
