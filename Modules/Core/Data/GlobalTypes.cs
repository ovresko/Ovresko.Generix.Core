using Ovresko.Generix.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ovresko.Generix.Core.Modules.Core.Data
{
    public class GlobalTypes : Dictionary<string,Type>
    {
        public Type Resolve(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ModuleClassNotFoundException("Can't resolve an empty type name!");

            Type type;
            bool isFound = this.TryGetValue(typeName, out type);
            if (isFound)
            {
                return type;
            }
            else
            {
                throw new ModuleClassNotFoundException($"Can't resolve {typeName}, type doesn't exist");
            }
        }
    }
}
