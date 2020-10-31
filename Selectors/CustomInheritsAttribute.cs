using System;
using System.Collections.Generic;
using TypeReferences;

namespace Unilonia.Selectors
{
    internal class CustomInheritsAttribute : TypeOptionsAttribute
    {
        public Type BaseClass { get; set; }

        public CustomInheritsAttribute(Type baseClass)
        {
            BaseClass = baseClass;
            Grouping = Grouping.None;
            ExcludeNone = true;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var toInclude = new List<Type>();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsSubclassOf(baseClass))
                        toInclude.Add(type);
                }
            }
            IncludeTypes = toInclude.ToArray();
        }

        public override bool MatchesRequirements(Type type)
        {
            return type.IsSubclassOf(BaseClass);
        }
    }
}
