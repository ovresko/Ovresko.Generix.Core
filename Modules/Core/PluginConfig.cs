using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ovresko.Generix.Core.Modules.Core
{
  public  class PluginConfig
    {
        public string Name { get; set; }
        public string Release { get; set; }
        public string Key { get; set; }
        public string User { get; set; }
        public string Version { get; set; }
        public string Link { get; set; }

        public List<Dependecie> Dependencies { get; set; }
    }

    public class Dependecie
    {
        public string Name { get; set; }
    }
}
