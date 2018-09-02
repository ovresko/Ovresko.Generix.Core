 using Ovresko.Generix.Core.Modules.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Ovresko.Generix.Core
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        

        protected override void OnStartup(StartupEventArgs e)
        {
                // MyEZLocalize = new EZLocalize(App.Current.Resources,
                //"fr",
                //null,
                //"Languages\\",
                //"InterfaceStrings");
            base.OnStartup(e);
        }
    }
}
