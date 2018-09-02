using Ovresko.Generix.Core.Modules;
using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Core.Pages.Startup;
using Stylet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unclassified.TxLib;
using static Ovresko.Generix.Core.Translations.OvTranslate;

namespace Ovresko.Generix.Core.Pages.LanuageChooser
{
    public class LangViewModel : Screen
    {

        public override Task<bool> CanCloseAsync()
        {
       
            try
            {
                Validate();
                return Task.Run<bool>(() => true);

            }
            catch (Exception s)
            {
                DataHelpers.ShowMessageError(s);
                return Task.Run<bool>(() => false);

            }

        }
        public LangViewModel( )
        {
            Langs = Tx.AvailableCultures.ToList();// CultureInfo.GetCultures(CultureTypes.AllCultures)?.ToList();
            SelectedLang = CultureInfo.CurrentCulture;
            NotifyOfPropertyChange("Langs");
        }

         public CultureInfo SelectedLang { get; set; }
        public List<CultureInfo> Langs { get; set; } = new List<CultureInfo>();


        public void Save()
        {
            try
            {
                Validate();

                var settings = ElvaSettings.getInstance();
                //settings.LangeDefault = SelectedLang;
                settings.Pays = SelectedLang?.Name;// CultureInfo.CurrentCulture?.Name;
                settings.Save();
                RequestClose(true);
                
            }
            catch (Exception s)
            {
                DataHelpers.ShowMessageError(s);
                return;
            }

        }

       

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(SelectedLang?.Name) == true)
                throw new Exception(_("Must choose a language!"));

        }
    }
}
