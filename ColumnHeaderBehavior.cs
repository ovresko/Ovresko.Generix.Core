using Ovresko.Generix.Core;
using Ovresko.Generix.Core.Modules.Core.Helpers;
using MongoDB.Bson;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using Ovresko.Generix.Core.Modules.Core.Module;
using Ovresko.Generix.Core.Modules.Core.Data;
using static Ovresko.Generix.Core.Translations.OvTranslate;
using Ovresko.Generix.Datasource.Services.Queries;
using Ovresko.Generix.Datasource.Models;

namespace AttributtedDataColumn
{
    public class ColumnHeaderBehavior : Behavior<DataGrid>
    {
           
        static GuidConv objectIdConv = new GuidConv();


        public static void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        { 
            var displayName = (e.PropertyDescriptor as PropertyDescriptor)?.Attributes[typeof(ExDisplayName)] as ExDisplayName ;// GetPropertyDisplayName(e.PropertyDescriptor);
            if (displayName == null)
            {
                e.Cancel=true;
                return;
            }

            
            var isShow = (e.PropertyDescriptor as PropertyDescriptor)?.Attributes[typeof(ShowInTableAttribute)] as ShowInTableAttribute; ;// GetPropertyDisplayName(e.PropertyDescriptor);
            if(isShow == null)
            {
                e.Cancel = true;
                return;
            }

            var columnAttr = (e.PropertyDescriptor as PropertyDescriptor)?.Attributes[typeof(ColumnAttribute)] as ColumnAttribute;
            bool estDevise = false;
            if (columnAttr != null) {

                if (columnAttr?.FieldType == ModelFieldType.Devise
                || (columnAttr.FieldType == ModelFieldType.ReadOnly && columnAttr.Options != ""))
                estDevise = true;                
            }

             
           

            if (displayName != null && !string.IsNullOrEmpty(displayName.DisplayName) && isShow.IsShow)
            {

                // IF IS EDITABLE DETAIL TABLE
                if ((sender as DataGrid).Name != "datagrid")
                {
                  if(  e.PropertyType == typeof(Guid))
                    {
                        e.Cancel = true;
                    }

                    if (e.Column.DependencyObjectType.SystemType == typeof(DataGridCheckBoxColumn)
                     || e.PropertyType == typeof(Guid)
                        || e.PropertyType == typeof(Guid)
                        || displayName.DisplayName == _("Réf"))
                    {
                        e.Column.IsReadOnly = true;
                    }
                    else
                    {
                        e.Column.IsReadOnly = false;
                    }
                }

                IsSourceAttribute attrisSource = (e.PropertyDescriptor as PropertyDescriptor).Attributes[typeof(IsSourceAttribute)] as IsSourceAttribute;

                // Treat column as combobox with source
                if (attrisSource != null)
                {
                    if (!String.IsNullOrWhiteSpace(attrisSource.source))
                    {
                        var cb = new DataGridComboBoxColumn();
                        cb.ItemsSource = DS.Generic(attrisSource.source)?.GetAll();
                        cb.DisplayMemberPath = $"Name";

                        cb.SelectedValuePath = "Id";
                        cb.SelectedValueBinding = new Binding($"l{attrisSource.source}");
                        e.Column = cb;
                    }
                }
                if (estDevise)
                {
                    DataGridTextColumn dataGridTextColumn = e.Column as DataGridTextColumn;
                    if (dataGridTextColumn != null)
                    { 
                        dataGridTextColumn.Binding.StringFormat = "N";
                    }
                }
                //if (e.PropertyType == typeof(DateTime))
                //{
                //    DataGridTextColumn dataGridTextColumn = e.Column as DataGridTextColumn;
                //    if (dataGridTextColumn != null)
                //    {
                //        dataGridTextColumn.Binding.StringFormat = "d";
                //    }
                //}
                e.Column.Header = displayName.DisplayName;
                e.Column.Width = DataGridLength.Auto;
                if (displayName.DisplayName == _("Crée le")
                    && ((sender as DataGrid).Name != "datagrid" || (sender as DataGrid).Tag?.ToString() == "report"))
                {
                    e.Cancel = true;
                    return;
                }
                if (displayName.DisplayName == _("Crée par")
                    && ((sender as DataGrid).Name != "datagrid" || (sender as DataGrid).Tag?.ToString() == "report"))
                {
                    e.Cancel = true;
                    return;
                }

                if (  displayName.DisplayName == _("Status")) //|| displayName == "Crée le"
                {
                    if ((sender as DataGrid).Name == "datagrid" && (sender as DataGrid).Tag?.ToString() != "report")
                    {

                        Style style = new Style()
                        {
                            TargetType = typeof(DataGridCell)
                        };


                       // style.Setters.Add(new Setter(property: DataGridCell.ForegroundProperty, value: Brushes.Black));
                        style.Setters.Add(new Setter(property: DataGridCell.MarginProperty, value: new Thickness(10, 2, 2, 2)));
                        style.Setters.Add(new Setter(property: DataGridCell.BackgroundProperty, value: new Binding { Converter = new BackColorConv() }));
                        style.Setters.Add(new Setter(property: DataGridCell.ForegroundProperty, value: System.Windows.Media.Brushes.White));

                        e.Column.CellStyle = style;
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }

                if ((sender as DataGrid).Name == "datagrid" )
                {
                    if (displayName.DisplayName == _("Réf"))
                    {
                        e.Column.DisplayIndex = 0;
                        Style style = new Style()
                        {
                            TargetType = typeof(DataGridCell),
                            BasedOn = App.Current.FindResource("MaterialDesignDataGridCell") as Style
                        };
                        // style.Setters.Add(new Setter(property: DataGridCell.MarginProperty, value: new Thickness(0)));
                        style.Setters.Add(new Setter(property: DataGridCell.VerticalContentAlignmentProperty, value: VerticalAlignment.Center));
                        style.Setters.Add(new Setter(property: DataGridCell.MinWidthProperty, value: ((double)150)));

                        // style.Setters.Add(new Setter(property: DataGridCell.PaddingProperty, value: new Thickness(10)));
                        style.Setters.Add(new Setter(property: DataGridCell.FontWeightProperty, value: FontWeights.SemiBold));
                        style.Setters.Add(new Setter(property: DataGridCell.VerticalAlignmentProperty, value: VerticalAlignment.Center));
                        e.Column.CellStyle = style; 
                    }

                    if (columnAttr != null
                        && !string.IsNullOrWhiteSpace(columnAttr.Options)
                        && (columnAttr.FieldType == ModelFieldType.Lien))
                    {
                        var c = ((e.Column as DataGridTextColumn).Binding as Binding);

                        c.ConverterParameter = columnAttr.Options;
                        c.Converter = objectIdConv;
                    }

                }

                if (displayName.DisplayName == "Retard")
                {
                    //Style style = new Style()
                    //{
                    //    TargetType = typeof(DataGridCell)
                    //};
                    //style.Setters.Add(new Setter(property: DataGridCell.ForegroundProperty, value: Brushes.Black));
                    //style.Setters.Add(new Setter(property: DataGridCell.MarginProperty, value: new Thickness(10, 2, 2, 2)));
                    //style.Setters.Add(new Setter(property: DataGridCell.BackgroundProperty, value: new Binding { Converter = new BackFicheAnimalColorConv() }));

                    //e.Column.CellStyle = style;
                }

                //var bold = GetPropertyIsBold(e.PropertyDescriptor);
                //if ((sender as DataGrid).Name == "datagrid" && bold)
                //{
                //    Style style = new Style()
                //    {
                //        TargetType = typeof(DataGridCell)
                //    };

                //    style.Setters.Add(new Setter(property: DataGridCell.FontWeightProperty, value: FontWeights.Bold));
                //    style.Setters.Add(new Setter(property: DataGridCell.VerticalAlignmentProperty, value: VerticalAlignment.Center));
                //    //style.Setters.Add(new Setter(property: DataGridCell.MarginProperty, value: new Thickness(10, 2, 2, 2)));
                //    e.Column.CellStyle = style;
                //}


            }
            else
            {
                e.Cancel = true;
            }

           
        }

        protected override void OnAttached()
        {
            AssociatedObject.AutoGeneratingColumn += new EventHandler<DataGridAutoGeneratingColumnEventArgs>(OnAutoGeneratingColumn);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.AutoGeneratingColumn -= new EventHandler<DataGridAutoGeneratingColumnEventArgs>(OnAutoGeneratingColumn);
        }


        private class GuidConv : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                try
                {

                    var v = GuidParser.Convert(value);// as Guid;
                    var param = parameter.ToString();
                    return v.GetObject(param)?.NameSearch;
                }
                catch
                {
                    return value;
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value;
                // throw new NotImplementedException();
            }
        }


        private class BackColorConv : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                try
                {
                    var v = (IDocument)value;                    
                    return v.StatusColor;
                }
                catch
                {
                    return Brushes.Black;
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value;
               // throw new NotImplementedException();
            }
        }

        private class BackFicheAnimalColorConv : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                try
                {
                    //var v = (Prestation)value;
                    //if (v.Retard.Equals("En Retard"))
                    //{
                    //    return Brushes.Orange;
                    //}
                    //else if (v.Retard == "Payé")
                    //{
                    //    return Brushes.LightGreen;
                    //}
                    //else if (v.Retard == "Programmé")
                    //{
                    //    return Brushes.White;
                    //}
                  
                    
                    return Brushes.WhiteSmoke;
                }
                catch
                {
                    return Brushes.Black;
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                //  throw new NotImplementedException();
                return value;
            }
        }
    }
}