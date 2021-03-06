using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ovresko.Generix.Core.Pages
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class ShellView  
    {
        public ShellView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(ShellView), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged), new CoerceValueCallback(OnCoerceScaleValue)));

        public static object OnCoerceScaleValue(DependencyObject o,object value)
        {
            ShellView shell = o as ShellView;
            if (shell != null)
                return shell.OnCoerceScaleValue((double)value);
            else
                return value;
        }

        private static void OnScaleValueChanged(DependencyObject o,DependencyPropertyChangedEventArgs e)
        {
            ShellView shell = o as ShellView;
            if (shell != null)
                shell.OnScaleValueChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual double OnCoerceScaleValue(double value)
        {
            if (double.IsNaN(value))
                return 1.0f;
            if (value <= 1)
                return value;

            if (value > 11)
                return 1;
            if (value <= 5)
                return 0.5;
            if (value <= 7)
                return 0.6;
            if (value <= 8)
                return 0.65;
            if (value <= 9)
                return 0.70;
            if (value <= 10)
                return 0.79;
            if (value <= 11)
                return 0.8;

            //Math.Min(1, value/10);

            return value;
        }

        protected virtual void OnScaleValueChanged(double oldValue,double newValue)
        {

        }

        public double ScaleValue
        {
            get
            {
                return (double)GetValue(ScaleValueProperty);
            }
            set
            {
                SetValue(ScaleValueProperty, value);
            }
        }

        private void CalculateScale()
        {
           // double yScale = ActualHeight / 100f;// 250f;
            double xScale = ActualWidth / 100f;// 200f;
            double value = xScale;// Math.Min(xScale, yScale);
            ScaleValue = (double)OnCoerceScaleValue(myMainWindow, value);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalculateScale();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
          
          //  main_tab.Items;
        }

        private void TextBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //var name = (sender as TextBlock).Text;
            //var nodeZ = (sender as TextBlock).GetParentObject();// (TextBlock.TextProperty).DataItem as TreeViewItemExCategory;//.NodeXml;
            //foreach (TreeViewItemExCategory item in MenuTree.Items)
            //{
            //    if (item.Name != name)
            //        SetTreeViewItems(item, false);
            //}
            
        }

        void SetTreeViewItems(object obj, bool expand)
        {
            if (obj is TreeViewItem)
            {
                ((TreeViewItem)obj).IsExpanded = expand;
                foreach (object obj2 in ((TreeViewItem)obj).Items)
                    SetTreeViewItems(obj2, expand);
            }
            else if (obj is ItemsControl)
            {
                foreach (object obj2 in ((ItemsControl)obj).Items)
                {
                    if (obj2 != null)
                    {
                        SetTreeViewItems(((ItemsControl)obj).ItemContainerGenerator.ContainerFromItem(obj2), expand);

                        TreeViewItem item = obj2 as TreeViewItem;
                        if (item != null)
                            item.IsExpanded = expand;
                    }
                }
            }
        }

        bool isOpen = true;
         
        public void OpenMenu()
        {
            //if(isOpen == false)
            //{
                DrawerHost.OpenDrawerCommand.Execute(null, mainDrawer);
                isOpen = true;
            //}
            //else
            //{
            //    DrawerHost.CloseDrawerCommand.Execute(null, mainDrawer);
            //    isOpen = false;
            //}
           
        }

        public void CloseMenu()
        {
            DrawerHost.CloseDrawerCommand.Execute(null, mainDrawer);
            //    isOpen = false;
        }


        public void Toggle()
        {  
                OpenMenu();
          
        }

        private  void ToggleButton_Click(object sender, RoutedEventArgs e)
        {

            //Toggle();
            //(this.DataContext as ShellViewModel).NotifyAppLogo();
        }
    }
}
