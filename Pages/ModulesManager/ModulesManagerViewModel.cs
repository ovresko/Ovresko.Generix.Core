using Microsoft.Win32;
using Ovresko.Generix.Core.Modules.Core;
using Stylet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ovresko.Generix.Core.Translations.OvTranslate;

namespace Ovresko.Generix.Core.Pages.ModulesManager
{
    public class ModulesManagerViewModel : Screen
    {

        public IWindowManager _windowManager { get; set; }

        #region PROPS


        public string ModulePath { get; set; }
        public bool ModulePayed { get; set; }
        public bool ModuleWithTemplate { get; set; }
        public string ModuleTitle { get; set; }
        public string ModuleVersion { get; set; }

        private bool FileLoded = false;

        public ModulesManagerViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }
        #endregion


        #region Pulic methods

        public void Fermer()
        {
            this.RequestClose();
        }

        public void Installer()
        {
            if (FileLoded && !string.IsNullOrWhiteSpace(ModulePath))
            {
                try
                {

                    FrameworkManager.InstallPlugin(ModulePath,_safeFileName, FrameworkManager.InstallMode.Zip);
                    var _doRestart = _windowManager.ShowMessageBox(_("Veuillez relancer l'application pour activer le module"), _("Relancer"), System.Windows.MessageBoxButton.OK);

                    //if (_doRestart == System.Windows.MessageBoxResult.Yes)
                    //{
                    //    System.Windows.Forms.Application.Restart();
                    //    System.Windows.Application.Current.Shutdown();
                    //}
                }
                catch (Exception s)
                {
                    _windowManager.ShowMessageBox(s.Message);
                    return;
                }
            }
            else
            {
                _windowManager.ShowMessageBox(_("Charger un module '.zip' pour l'installer"));
                return;
            }
        }

        private string _safeFileName;
        public void Parcourir()
        {
            var dialog = (new OpenFileDialog());
            dialog.Title = _("Selectionner un module .ZIP");
            dialog.Filter = "Zip Files|*.zip";

            var result = dialog.ShowDialog();
            if (result == true)
            {
                _safeFileName = dialog.SafeFileName;

                var file = dialog.FileName;
                var fullPath = Path.GetFullPath(file);
                if (File.Exists(fullPath))
                {
                    ModulePath = fullPath;
                    NotifyOfPropertyChange("ModulePath");
                    FileLoded = true;
                    return;
                }
            }

            FileLoded = false;
        }
        #endregion
    }
}
