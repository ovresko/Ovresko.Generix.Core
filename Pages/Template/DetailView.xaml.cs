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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ovresko.Generix.Core.Pages.Template
{
    /// <summary>
    /// Logique d'interaction pour DetailView.xaml
    /// </summary>
    public partial class DetailView : UserControl
    {
        public DetailView()
        {
            InitializeComponent();
        }

        private void button_Copy1_Click(object sender, RoutedEventArgs e)
        {

           // var win = Window.GetWindow(this);
            //if(win.)
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var offset = scroller.VerticalOffset;
            (this.DataContext as DetailViewModel).SidePanelMargin = new  Thickness(20, offset,20,20);
        }
    }
}
