using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ovresko.Generix.Core.Modules.Core.Helpers
{
    public static class StringExtentions
    {

        public static bool ContainsIgniorCase(this string value, string tocompareagainst)
        {
            if (tocompareagainst == null)
                return false;

            return (value?.ToLower()?.Contains(tocompareagainst?.ToLower()) == true);
        }
       
    }
}
