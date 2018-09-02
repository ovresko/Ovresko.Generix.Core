using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ovresko.Generix.Core.Translations.OvTranslate;

namespace Ovresko.Generix.Core.Modules.Core.Module
{
   public static class ModuleIcons
    {
        public static Dictionary<string, string> ModuleIcon { get; set; } = new Dictionary<string, string>
        {
            {_("Application"),"Altimeter" },
            {_("Framework"),"EngineOutline" },
            {_("Systémes"),"AutoFix" }
            
        };
    }
}

