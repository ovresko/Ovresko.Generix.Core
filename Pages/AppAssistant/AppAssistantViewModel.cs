using Microsoft.Win32;
using Ovresko.Generix.Core.Modules;
using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Utils.Data.Import;
using Stylet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using static Ovresko.Generix.Core.Translations.OvTranslate;

namespace Ovresko.Generix.Core.Pages.AppAssistant
{
    internal class AppAssistantViewModel : Screen
    {
        private IWindowManager _windowManager;
        private int PageCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="windowManager">Stylet Window Manager</param>
        public AppAssistantViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager; 
            EmailList = new List<EmailProvider>();

            // If Number of page ==1 then the button text value should be Terminé
            if (PageCount < 2)
            {
                // Do the Update;
                UpdateButtonNextValue();
            }

            // Load Liste of present Cultures
            LoadCultureInfos();

            // Set Default Culture as
            //SelectedPaye = CultureInfo.CurrentCulture;
            //NotifyOfPropertyChange("SelectedPaye");
             
        }


        public void Open()
        {
            this._windowManager.ShowDialog(this);
        }

        public string AppVersion
        {
            get
            {
                return $"{Assembly.GetExecutingAssembly().GetName().Version}";
            }
        }

        // Settings Properties
        #region Properties
        //public List<string> DeviseList { get; set; }
        public string Email { get; set; }
        public List<EmailProvider> EmailList { get; set; }
        public string EmailPassword { get; set; }
        public string EmailProvider { get; set; }
        public string Entreprise { get; set; }
        public int indexPage { get; set; } = 0;
        public string LogoPath { get; set; }
        public string NextButtonText { get; set; } = _("Suivant");
        public string PasswordAdmin { get; set; }
        public List<CultureInfo> PaysList { get; set; }
        //public Devise SelectedDevise { get; set; }
        public EmailProvider SelectedEmail { get; set; }
        //public CultureInfo SelectedPaye { get; set; }
        private int LastPageIndex { get { return PageCount - 1; } }
        //public List<string> LangList { get; set; } = new List<string> { "fr", "en", "ar" };
        //public string SelectedLang { get; set; }  
        #endregion


        /// <summary>
        /// Select Application Logo to display
        /// </summary>
        public void AddLogo()
        {
            var file = "";

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = _("Selectionner une image");
            ofd.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
                "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                "Portable Network Graphics (*.png)|*.png";
            BitmapImage img;
            if (ofd.ShowDialog() == true)
            {
                LogoPath = file = System.IO.Path.GetFullPath(ofd.FileName);
                NotifyOfPropertyChange("LogoPath");

                //try
                //{
                //    file = System.IO.Path.GetFullPath(ofd.FileName);
                //    var imageLink = $"img_data/{propName}_{DateTime.Now.Year}_{DateTime.Now.DayOfYear}_{DateTime.Now.Millisecond}_{System.IO.Path.GetExtension(ofd.FileName)}";

                //    if (!string.IsNullOrWhiteSpace(file))
                //    {
                //        // var img = new BitmapImage(new Uri(varimgPath));
                //        var Bitm = new System.Drawing.Bitmap(file);
                //        ImageTools.Save(Bitm, 150, 200, imageLink);
                //    }

                //    // File.Copy(file, imageLink, true);
                //    img = new BitmapImage(new Uri(System.IO.Path.GetFullPath(imageLink)));

                //    Nmodel.GetType().GetProperty(propName).SetValue(model, imageLink);
                //    NotifyOfPropertyChange("model");
                //    await Setup(SetupBypass: true);
                //}
                //catch (Exception s)
                //{
                //     DataHelpers.ShowMessage(s.Message);
                //}
            }
        }

        /// <summary>
        /// Move to the Next page
        /// </summary>
        public void Next()
        {
            // Check if we're in the last page
            if (indexPage == LastPageIndex)
            {
                // save everything we're done
                ElvaSettings settings = DataHelpers.Settings;

                //if (SelectedPaye == null)
                //{
                //    DataHelpers.ShowMessage(_("Culture is empty"));
                //    return;
                //}

                settings.Societe = Entreprise;
                settings.Email = Email;
                settings.EmailPwd = EmailPassword;
                settings.AppInitialized = true;
               

                // Edit the user password
                User  user = DS.db.GetOne<User>(a => a.Name == "Admin");
                if  (null != user && !string.IsNullOrEmpty(PasswordAdmin))
                {
                    user.Password = PasswordAdmin;
                    user.Save();
                }

                TransfertLogo();
                // todo: add mapping between email provider and email port, host adresse
                settings.Save();

                // Ask if we should close the window
                var _doIExit = _windowManager.ShowMessageBox(_("Voulez-vous fermer la fenêtre ?"), _("Fermeture"), System.Windows.MessageBoxButton.YesNo);
                if (_doIExit == System.Windows.MessageBoxResult.Yes)
                {
                    // Close the Assistant
                    this.RequestClose();
                    return;
                }
            }

            // Movin to th next page
            else
            {
                // Valdiate this page before moving to next one
                ValidatePage(indexPage);

                // Select next tabs
                indexPage++;
                NotifyOfPropertyChange("indexPage");

                // Update next button value text
                UpdateButtonNextValue();
            }
        }

        /// <summary>
        /// Go to the previous page
        /// </summary>
        public void Return()
        {
            // Chek if th first one
            if (indexPage <= 0)
                return;

            // Select previous tabs
            indexPage--;
            NotifyOfPropertyChange("indexPage");

            //Update Button Next text Value
            UpdateButtonNextValue();
        }

        /// <summary>
        /// Load Cultures From curretn machine and add it
        /// </summary>
        private void LoadCultureInfos()
        {
            PaysList = new List<CultureInfo>();
            var inMachine = CultureInfo.GetCultures(CultureTypes.AllCultures);
            if (inMachine != null)
            {
                foreach (var item in inMachine)
                {
                    PaysList.Add(item);
                }
                // Notify
                NotifyOfPropertyChange("PaysList");
            }
        }

        /// <summary>
        /// Open ovrsko.com
        /// </summary>
        public void OpenSite()
        {
            System.Diagnostics.Process.Start("http://www.ovresko.com");

        }

        /// <summary>
        /// Copy image to the img_data and set settings logo path to it's value
        /// </summary>
        private void TransfertLogo()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(LogoPath) == false && File.Exists(LogoPath))
                {
                    var file = System.IO.Path.GetFullPath(LogoPath);
                    var imageLink = $"Database/images/AppLogo_{DateTime.Now.Year}_{DateTime.Now.DayOfYear}_{DateTime.Now.Millisecond}_{System.IO.Path.GetExtension(LogoPath)}";

                    if (!string.IsNullOrWhiteSpace(file))
                    {
                        // var img = new BitmapImage(new Uri(varimgPath));
                        var Bitm = new System.Drawing.Bitmap(file);
                        ImageTools.Save(Bitm, 150, 200, imageLink);
                    }

                    // File.Copy(file, imageLink, true);
                    var img = new BitmapImage(new Uri(System.IO.Path.GetFullPath(imageLink)));

                    DataHelpers.Settings.AppLogo = imageLink;
                }
            }
            catch (Exception s)
            {
                _windowManager.ShowMessageBox(s.Message);
            }
        }
        /// <summary>
        /// Update Button text from next to finish
        /// </summary>
        private void UpdateButtonNextValue()
        {
            // Check if we're in the one before
            if (indexPage == LastPageIndex)
            {
                NextButtonText = _("Terminer");
                NotifyOfPropertyChange("NextButtonText");
            }
            else
            {
                NextButtonText = _("Suivant");
                NotifyOfPropertyChange("NextButtonText");
            }
        }

        /// <summary>
        /// Valdiate #pageNumber page fields
        /// </summary>
        /// <param name="pageindex">0 based page index to validate </param>
        private void ValidatePage(int pageindex)
        {
            //todo: Chek valdiation of fields per page inex
        }
    }
}