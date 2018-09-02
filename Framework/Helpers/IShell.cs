using Ovresko.Generix.Core.Modules.Core.Module;
using Ovresko.Generix.Core.Modules.Core.Module;
using Ovresko.Generix.Datasource.Models;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ovresko.Generix.Core.Modules.Core.Helpers
{
   public interface IShell
    {
        void OpenScreen(IScreen screen, string title);
        void CloseScreen(IScreen screen);
        Task OpenScreenDetach(IDocument doc, string s);
    }
}
