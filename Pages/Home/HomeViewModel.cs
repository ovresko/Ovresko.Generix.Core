using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Core.Modules;
using MaterialDesignThemes.Wpf;
using Stylet;
using Stylet.Logging;
using System;
using System.Collections.Generic; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static Ovresko.Generix.Core.Translations.OvTranslate;
using System.Windows.Media;
using System.IO;
using Ovresko.Generix.Core.Modules.Core.Module;
using Ovresko.Generix.Core.Exceptions;
using Unclassified.TxLib;
using Ovresko.Generix.Datasource.Models;

namespace Ovresko.Generix.Core.Pages.Home
{
    class HomeViewModel : Screen, IDisposable
    { 
        public string LogoSrc { get; set; }

        //public string Applogo
        //{
        //    get
        //    {
        //        if (DataHelpers.Settings.AppLogo != null)
        //            return Path.GetFullPath(DataHelpers.Settings.AppLogo);
        //        return "";
        //    }
        //}
        ///public ScrollViewer MasterPanel { get; set; } = new ScrollViewer();
        public StackPanel MasterPanelStack { get; set; } = new StackPanel();
        public WrapPanel MasterPanel { get; set; }


        protected override async void OnActivate()
        {
            base.OnActivate();
            await Setup();
        }

        public HomeViewModel()
        {
            DisplayName = "Home";
            try
            {
                if (DataHelpers.Settings.AppLogo != null)
                {
                    LogoSrc= Path.GetFullPath(DataHelpers.Settings.AppLogo);
                    NotifyOfPropertyChange("LogoSrc");
                }

                //AppBackground = Path.GetFullPath(ElvaSettings.getInstance().AppBackground);
                //NotifyOfPropertyChange("AppBackground");
            }
            catch (Exception s)
            {
                DataHelpers.Logger.LogError(s);
            }
        }

        //public string AppBackground { get; set; }

        private async Task Setup()
        {
            MasterPanel = new WrapPanel();
            MasterPanel.Margin = new Thickness(15);
             
            // Init master panel
            MasterPanelStack = new StackPanel();
            MasterPanelStack.Orientation = Orientation.Vertical;
            MasterPanelStack.VerticalAlignment = VerticalAlignment.Stretch;
            MasterPanelStack.HorizontalAlignment = HorizontalAlignment.Stretch;

            // get Groupe names
            var groupes = DataHelpers.Modules.Where(z => !string.IsNullOrWhiteSpace(z.GroupeModule)).Select(a => a.GroupeModule).Distinct();

            foreach (var group in groupes)
            {
                //HomeIcons = new WrapPanel();
                //HomeIcons.HorizontalAlignment = HorizontalAlignment.Stretch;
                var modules = DataHelpers.Modules.Where(a => a.EstAcceRapide && a.GroupeModule == group);

                //Add groupe name to stack
                if(modules.Any())
                    MasterPanelStack.Children.Add(new TextBlock() { Text = group, Foreground=Brushes.White });
                
                foreach (var item in modules)
                {

                    Button btn = new Button();
                    btn.HorizontalAlignment = HorizontalAlignment.Center;
                    btn.HorizontalContentAlignment = HorizontalAlignment.Center;
                    //btn.Content = item.Libelle;
                    btn.Style = App.Current.FindResource("HomeButton") as Style;
                    btn.Tag = item;
                    btn.Click += Btn_Click;
                  //  btn.TouchDown += Btn_Click;
                    var ic = item.ModuleIcon;
                    StackPanel sp = new StackPanel() { Orientation = Orientation.Vertical };
                    sp.HorizontalAlignment = HorizontalAlignment.Center;

                    if (!string.IsNullOrWhiteSpace(ic))
                    {
                        // add icon
                        PackIcon pi = new PackIcon();
                        try
                        {
                            pi.Kind = (PackIconKind)Enum.Parse(typeof(PackIconKind), ic);
                        }
                        catch (Exception s)
                        {
                            DataHelpers.Logger.LogError(s.Message);
                            pi.Kind = PackIconKind.Exclamation;
                            DataHelpers.Logger.LogInfo($"Setting default Icon for {item.Libelle }");
                        }
                        pi.Width = 50;
                        pi.HorizontalAlignment = HorizontalAlignment.Center;
                        pi.Height = 50;

                        var spB = new StackPanel();

                        if (item.IsInstanceModule == false && !item.ClassName.Contains("_report"))
                        {
                            var plusBtn = new Button();
                            plusBtn.Tag = item;
                            plusBtn.ToolTip = $"{_("home.btn.plus")} {item.Libelle}";
                            plusBtn.Style = App.Current.FindResource("HomeButtonPlus") as Style; ;
                            plusBtn.Content = "+";
                            plusBtn.Click += PlusBtn_Click;
                            spB.Children.Add(pi);
                            spB.Children.Add(plusBtn);
                        }
                        else
                        {
                            spB.Children.Add(pi);
                        }

                        btn.Content = (spB  );
                    }
                   
                     
                    sp.Children.Add(btn);
                    sp.Children.Add(new TextBlock() { Text = item.Libelle,FontWeight =  FontWeights.DemiBold,Foreground = Brushes.Gray, HorizontalAlignment = HorizontalAlignment.Center });
                    sp.MouseEnter += Sp_MouseEnter;
                    sp.MouseLeave += Sp_MouseLeave;
                    // btn.Content = ic;
                    MasterPanel.Children.Add(sp);
                   // MasterPanelStack.Children.Add(HomeIcons);
                }

               
            }
           


            NotifyOfPropertyChange("MasterPanel");
        }

        private void Sp_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                var sp = sender as StackPanel;

                var btn = sp?.Children[0] as Button;
                var ic = btn?.Content as StackPanel;
                var icon = ic?.Children[0] as PackIcon;
                //btn.Width -= 5;
                // btn.Height -= 5;
                icon.Width -= 5;
                icon.Height -= 5;
            }
            catch (Exception s)
            {
                Console.WriteLine(s.Message);
            }
        }

        private void Sp_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            try
            {
                var sp = sender as StackPanel;
                // sp.Width += 15;

                // var label = sp.Children[1] as TextBlock;
                //label.FontSize += 3;

                var btn = sp?.Children[0] as Button;
                var ic = btn?.Content as StackPanel;
                var icon = ic?.Children[0] as PackIcon;
                //btn.Width -= 5;
                // btn.Height -= 5;
                icon.Width += 5;
                icon.Height += 5;
            }
            catch (Exception s)  
            {
                Console.WriteLine(s.Message );
             }
        }

        private async void PlusBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var modulerp = (sender as Button).Tag as ModuleErp;
                
                Type type = DataHelpers.GetTypesModule.Resolve(modulerp.ClassName);
                
                // Type.GetType(modulerp.ClassName);
                var instance = Activator.CreateInstance(type);
                if (instance != null)
                {
                    var model = (instance as IDocument);
                    DataHelpers.Shell.OpenScreenAttach(model, model.Name);
                }

            }
            catch (Exception s)
            {
                DataHelpers.ShowMessageError(s);
            }
        }

        private async void Btn_Click(object sender, EventArgs e)
        {
            try
            {
                if (((e as RoutedEventArgs).OriginalSource as Button).Content?.ToString() == "+")
                    return;
            }
            catch  
            {
                return;
            }
            try
            {
                var modulerp = (sender as Button).Tag as ModuleErp;
                await DataHelpers.Shell.OpenModuleErp(modulerp);
            }
            catch (Exception s)
            {
                DataHelpers.Logger.LogError(s);
                 DataHelpers.ShowMessage( s.Message);
                return;
            }
        }

        public void Dispose()
        {

        }
    }
}
