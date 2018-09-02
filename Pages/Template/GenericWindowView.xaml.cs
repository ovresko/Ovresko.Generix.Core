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

namespace Ovresko.Generix.Core.Pages.Template
{
    /// <summary>
    /// Logique d'interaction pour GenericWindowView.xaml
    /// </summary>
    public partial class GenericWindowView : Window
    {
        public GenericWindowView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ScaleValueProperty = DependencyProperty.Register("ScaleValue", typeof(double), typeof(GenericWindowView), new UIPropertyMetadata(1.0, new PropertyChangedCallback(OnScaleValueChanged), new CoerceValueCallback(OnCoerceScaleValue)));

        public static object OnCoerceScaleValue(DependencyObject o, object value)
        {
            GenericWindowView shell = o as GenericWindowView;
            if (shell != null)
                return shell.OnCoerceScaleValue((double)value);
            else
                return value;
        }

        private static void OnScaleValueChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            GenericWindowView shell = o as GenericWindowView;
            if (shell != null)
                shell.OnScaleValueChanged((double)e.OldValue, (double)e.NewValue);
        }

        protected virtual double OnCoerceScaleValue(double value)
        {
            if (double.IsNaN(value))
                return 1.0f;

            if (value > 1)
                value = 1;
            if (value <= 1)
                value = Math.Max(0.92, value);

            return value;
        }

        protected virtual void OnScaleValueChanged(double oldValue, double newValue)
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
            double yScale = ActualHeight / 880f;// 250f;
            double xScale = ActualWidth / 704f;// 200f;
            double value = Math.Min(xScale, yScale);
            ScaleValue = (double)OnCoerceScaleValue(myMainWindow, value);
        }

        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CalculateScale();
        }
    }
}
