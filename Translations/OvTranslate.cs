using Ovresko.Generix.Core.Modules;
using Ovresko.Generix.Core.Modules.Core.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unclassified.TxLib;

namespace Ovresko.Generix.Core.Translations
{

    public static class OvTranslate
    {


        public static string _(string value)
        {
            //if (string.IsNullOrWhiteSpace(value))
            //    return value;
            try
            { 
                return Tx.SafeText(value); 
            }
            catch
            { 
                return value;
            }
        }

        public static string _(string value,int count)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;
            try
            {
                return Tx.TC(value,count);
            }
            catch
            {
                return value;
            }
        }
    }
}
