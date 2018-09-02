using Ovresko.Generix.Core.Modules.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ovresko.Generix.Core.Pages
{
    /// <summary>
    /// Logique d'interaction pour LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView()
        {
           
            InitializeComponent();
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
          //  var val = App.MyEZLocalize.LanguagesForFileBaseName("Languages", "InterfaceStrings");
          // var s= this.TryFindResource("HelloStr") as string;

          //string error =  App.MyEZLocalize.ChangeLanguage("en");
          //  string val2 = this.TryFindResource("HelloStr") as string;

        }

        private void pwdBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            (this.DataContext as LoginViewModel).pwd = pwdBox.Password;
        }
    }
}
