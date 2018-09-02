 using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging; 
using System.IO.Compression;
using Ovresko.Generix.Core.Modules.Core;

namespace Ovresko.Generix.Core.Pages.ModulesManager
{
    /// <summary>
    /// Logique d'interaction pour ModulesManagerView.xaml
    /// </summary>
    public partial class ModulesManagerView : Window
    {
        private SaveFileDialog saveFileDialog1 = new SaveFileDialog();
        private string filepath;

        public ModulesManagerView()
        {
            InitializeComponent();
            
        }
  
    }
}
