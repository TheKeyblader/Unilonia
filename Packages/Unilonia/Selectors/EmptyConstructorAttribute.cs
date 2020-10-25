using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeReferences;

namespace Unilonia.Selectors
{
    public class EmptyConstructorAttribute : TypeOptionsAttribute
    {
        public Type BaseClass { get; set; }

        public EmptyConstructorAttribute(Type baseClass)
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
                    if (type.IsSubclassOf(baseClass) &&
                        !type.IsGenericType &&
                        type.IsPublic &&
                        type.GetConstructors().Any(c => c.GetParameters().Length == 0))
                        toInclude.Add(type);
                }
            }
            IncludeTypes = toInclude.ToArray();
        }

        public override bool MatchesRequirements(Type type)
        {
            return type.IsSubclassOf(BaseClass) &&
                !type.IsGenericType &&
                type.IsPublic &&
                type.GetConstructors().Any(c => c.GetParameters().Length == 0);
        }
    }
}
