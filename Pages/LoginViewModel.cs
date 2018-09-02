using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Core.Modules;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Ovresko.Generix.Core.Translations.OvTranslate;
using System.Reflection;
using Unclassified.TxLib;

namespace Ovresko.Generix.Core.Pages
{
    class LoginViewModel : Screen, IDisposable
    {

        public LoginViewModel()
        {
           
        }
        public string pwd { get; set; }
        public User user { get; set; }
        public bool Logedin { get; set; }

        public string AppVersion { get
            {
                return $"{Assembly.GetExecutingAssembly().GetName().Version}";
            }
        }
        public List<User> users { get
            {
                return (DS.db.GetAll<User>(a => true));
            }
        }
        public string txtUtilisateur
        {
            get
            {
                return _("Utilisateur");
            }
        }
        public string txtPwd
        {
            get
            {
                return _("Mots de passe");
            }
        }
        public string txtConnecter { get
            {
                return _("Connecter");
            }
        }

        public string txtFermer { get { return _("Fermer"); } }

        public void Connecter()
        {
            if (user == null)
            {
                DataHelpers.ShowMessage(_("login.msg.select.user"));
                return;
            }

            //if (string.IsNullOrWhiteSpace(pwd))
            //    return;

            Logedin = pwd == (user.Password);
          
            if (Logedin )
            {
                DataHelpers.ConnectedUser = user;
                RequestClose(true);
            }
            else
            {
                 DataHelpers.ShowMessage( _("Mots de pass incorrect!"));
                return;
            }
        }


        public void Close()
        {
            RequestClose(false);
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
