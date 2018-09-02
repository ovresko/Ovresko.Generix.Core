using Ovresko.Generix.Core.Modules.Core;
using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Core.Modules;
using Microsoft.Win32;
using Portable.Licensing;
using Stylet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ovresko.Generix.Core.Pages.LicenceManage
{
    class LicenceManagerViewModel : Screen
    {

        public string licenceFile { get; set; }
        public string clePublic { get; set; }
        public string email { get; set; }
        public string userName { get; set; }

        public string TypeLicence { get; set; }
        public string DateExpiration { get; set; }


        public LicenceManagerViewModel()
        {
            //if (File.Exists("public-lcs"))
            //{
            //    using(var str = new StreamReader("public-lcs"))
            //    {
            //        clePublic = str.ReadToEnd();
                   
            //    }
            //}

            //if (File.Exists("License.lic"))
            //{
            //    try
            //    {
            //        using (var sr = (new StreamReader("License.lic")))
            //        {
            //            var lic = License.Load(sr);
            //            TypeLicence = lic.Type.ToString();
            //            DateExpiration = lic.Expiration.ToLongDateString();
            //            IsValide = true;
            //        }
            //    }
            //    catch 
            //    {}
            //}

            //var settings = DataHelpers.Settings;
            //email = settings.Email;
            //userName = settings.UserName;
            //NotifyOfPropertyChange("clePublic");
            //NotifyOfPropertyChange("email");
            //NotifyOfPropertyChange("userName");
            //NotifyOfPropertyChange("TypeLicence");
            //NotifyOfPropertyChange("DateExpiration");
        }

        public void LoadLicence()
        {
            var od = new OpenFileDialog();
            if (od.ShowDialog() == true)
            {
                var file = od.FileName;
                if (!file.Contains("lic"))
                {
                     DataHelpers.ShowMessage( "Licence non valide");
                    return;
                }
                licenceFile = file;
                NotifyOfPropertyChange("licenceFile");
            }
        }


        public bool IsValide { get; set; }

        public void VlidateLicence()
        {
            //if(string.IsNullOrWhiteSpace( licenceFile))
            //{
            //     DataHelpers.ShowMessage( "Charger votre licence d'abord");
            //    return;
            //}
            //if (string.IsNullOrWhiteSpace(clePublic))
            //{
            //     DataHelpers.ShowMessage( "Clé public obligatoire!");
            //    return;
            //}

            //if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(userName))
            //{
            //     DataHelpers.ShowMessage( "Vérifer nom d'utilsateur ou email");
            //    return;
            //}

            //using (var publicK = new StreamWriter("public-lcs"))
            //{
            //    publicK.Write(clePublic);
            //    publicK.Close();
            //}
              

            //File.Copy(licenceFile, "License.lic", true);

            //try
            //{
            //    if (FrameworkManager.ValidateLicence(clePublic,userName,email))
            //    {
            //         DataHelpers.ShowMessage( "Licence validée");
            //        var settings =  ElvaSettings.getInstance();
            //        settings.UserName = userName;
            //        settings.Email = email;
            //        settings.Save();
            //        IsValide = true;
            //        this.RequestClose();
            //    }
            //    else
            //    {
            //        IsValide = false;
            //         DataHelpers.ShowMessage( "Licence invalide");

            //    }
            //}
            //catch (Exception s)
            //{
            //     DataHelpers.ShowMessage( s.Message);
            //}



        }

        protected override void OnClose()
        {
            if (!IsValide)
            {
                Environment.Exit(0);
            }

            base.OnClose();
        }

        public void ActivateDemo()
        {
            if (string.IsNullOrWhiteSpace(clePublic))
            {
                 DataHelpers.ShowMessage( "Clé public obligatoire!");
                return;
            }
            using (var publicK = new StreamWriter("public-lcs"))
            {
                publicK.Write(clePublic);
                publicK.Close();
            }
               
            var used =  ElvaSettings.getInstance().DemoUsed;
            if (false)
            {
                 DataHelpers.ShowMessage( "Licence demo expirée, Contactez votre fournisseur/ 0665 97 76 79 / ovresko@gmail.com");
                return;
            }
            else
            {
                FrameworkManager.CreateLicenceTrial(userName, email);
                try
                {
                    if (FrameworkManager.ValidateLicence(clePublic,userName,email))
                    {
                         DataHelpers.ShowMessage( "Licence validée");
                        IsValide = true;
                        this.RequestClose();
                    }
                    else
                    {
                         DataHelpers.ShowMessage( "Licence invalide");
                        IsValide = false;
                        
                    }
                }
                catch (Exception s)
                {
                     DataHelpers.ShowMessage( s.Message);
                }
            }
        }

    }
}
