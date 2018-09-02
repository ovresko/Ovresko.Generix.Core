using Ovresko.Generix.Core.Modules.Core.Helpers;
using MongoDB.Bson;
 using Stylet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Core.Modules.Core.Module;
using System.Windows.Controls;
using static Ovresko.Generix.Core.Translations.OvTranslate;
using Ovresko.Generix.Datasource.Models;
using Ovresko.Generix.Datasource.Services.Queries;

namespace Ovresko.Generix.Core.Pages.Template
{
    public class FiltreViewModel<T> : Screen, IDisposable where T : IDocument, new()
    {
        public object Model { get; set; }
        public Dictionary<string, PropertyInfo> Properties { get; set; } = new Dictionary<string, PropertyInfo>();
        private PropertyInfo _SelectedProeprty;
        private Thread t;
        private List<object> allData;

        public PropertyInfo SelectedProeprty
        {
            get
            {

                return _SelectedProeprty;
            }
            set
            {
                _SelectedProeprty = value;
                t = new Thread(new ThreadStart(PopulatePossibleValues));
                t.Start();
            }
        }

        public string StatusLabel { get; set; }


        public void PopulatePossibleValues()
        {
            // Get possible values
            PossibleValues.Clear();
            var type = GetTypeName(SelectedProeprty.PropertyType);
            HashSet<string> collections = new HashSet<string>();
            foreach (var item in Inputs)
            {
                var val = _SelectedProeprty.GetValue(item, null)?.ToString();
                if (val != null)
                {
                    if (type == "Guid")
                    {
                        try
                        {
                            var attributes = _SelectedProeprty.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
                            var option = attributes.Options;
                            if (string.IsNullOrWhiteSpace(option) || option.Contains(">") || string.IsNullOrWhiteSpace(val))
                                continue;
                            var link = DS.Generic(option)?.GetById(GuidParser.Convert(val));//, true) as IEnumerable<IDocument>;
                            if (link == null)
                                continue;
                            collections.Add(link?.Name);
                            StatusLabel = $"{_("Récupération des données")} - {link?.Name}";
                            NotifyOfPropertyChange("StatusLabel");
                        }
                        catch (Exception s)
                        {
                            DataHelpers.ShowMessage(s.Message);
                        }
                    }
                    else
                    {
                        collections.Add(val as string);
                    }
                }
            }
            PossibleValues.AddRange(collections);
            StatusLabel = _($"Términé");
            NotifyOfPropertyChange("StatusLabel");
            NotifyOfPropertyChange("PossibleValues");
        }

        public List<T> Inputs { get; set; }
        public List<T> Result { get; set; } = new List<T>();
        public BindableCollection<string> PossibleValues { get; set; } = new BindableCollection<string>();
        public string Valeur { get; set; }

        // Conditions
      //  public string ConditionsSelected { get; set; } = _("ressemble");


        public enum EnumConditions
        {
            egale,
            ressemble,
            déffirent,
            inférieur,
            supérieur
        }

        public BindableCollection<LabelledValue<EnumConditions>> Conditions { get; private set; }
        public LabelledValue<EnumConditions> ConditionsSelected { get; set; } 

        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();
            this.Conditions = new BindableCollection<LabelledValue<EnumConditions>>()
              {
                 LabelledValue.Create(_("égale"), EnumConditions.egale),
                 LabelledValue.Create(_("ressemble"), EnumConditions.ressemble),
                 LabelledValue.Create( _( "déffirent de"), EnumConditions.déffirent),
                 LabelledValue.Create(_("inférieur à"), EnumConditions.inférieur),
                 LabelledValue.Create(  _("supérieur à"), EnumConditions.supérieur),
              };

            this.ConditionsSelected = this.Conditions[1];

        }


        //public IEnumerable<string> Conditions
        //{
        //    get
        //    {
        //        return new List<string>()
        //        {
        //            _("égale"),
        //            _("ressemble"),
        //           _( "déffirent de"),
        //            _("inférieur à"),
        //            _("supérieur à")
        //        };
        //    }
        //}


        public void SetInputs(List<T> items)
        {
            Inputs = new List<T>();
            Inputs = items;
            if (Inputs != null && Inputs.Count > 0)
            {
                var atype = Inputs[0];

                Type t = atype.GetType();
                PropertyInfo[] props = t.GetProperties();
                Dictionary<string, object> dict = new Dictionary<string, object>();
                foreach (PropertyInfo prp in props)
                {
                    //
                    //|| ((ColumnAttribute)a).FieldType == ModelFieldType.Lien

                    if (prp.Name == "Name")
                    {
                        Properties.Add(_("Réf"), prp);
                    }

                    var forbiden = new string[] { ">", "<", "()", "_" };
                    Object[] attrs = prp.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                    Object[] sepa = prp.GetCustomAttributes(typeof(ColumnAttribute), true);
                    var attValide = sepa.Where(a => ((ColumnAttribute)a).FieldType == ModelFieldType.Separation
                    || ((ColumnAttribute)a).FieldType == ModelFieldType.Button
                    || ((ColumnAttribute)a).FieldType == ModelFieldType.OpsButton
                    || ((ColumnAttribute)a).FieldType == ModelFieldType.Image
                    || ((ColumnAttribute)a).FieldType == ModelFieldType.Table
                    || ((ColumnAttribute)a).FieldType == ModelFieldType.LienButton
                    || forbiden.Contains(((ColumnAttribute)a).Options));
                    if (attValide.Any())
                        continue;

                    foreach (var att in attrs)
                    {
                        DisplayNameAttribute attribute = att as DisplayNameAttribute;
                        if ((attribute != null) && (attribute != DisplayNameAttribute.Default))
                        {
                            try
                            {
                                Properties.Add(attribute.DisplayName, prp);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
        }

        public WrapPanel Filtres { get; set; }
        public FiltreViewModel(WrapPanel Filtres)
        {
            this.Filtres = Filtres;
            NotifyOfPropertyChange("Filtres");
        }

        public FiltreViewModel(List<object> allData)
        {
            this.allData = allData;
        }

        public void doValider()
        {
            if (SelectedProeprty == null  )
            { RequestClose(false); }
            else
            {
                FilterData();
                try { this.RequestClose(true); } catch { }
            }

        }


        public void doValiderAndNext()
        {
            FilterData();
            //ConditionsSelected = String.Empty;
            Valeur = String.Empty;

            NotifyOfPropertyChange("SelectedProeprty");
            NotifyOfPropertyChange("ConditionsSelected");
            NotifyOfPropertyChange("Valeur");

        }

        public void FilterData()
        {
            if (SelectedProeprty == null)
            {
                DataHelpers.ShowMessage(_("Données Insuffisantes"));
                return;
            }

            if (ConditionsSelected == null)
            {
                DataHelpers.ShowMessage(_("Vérifier les conditions"));
                return;
            }
            if (string.IsNullOrEmpty(Valeur))
            {
                Valeur = " ";
                //DataHelpers.ShowMessage("Vérifier les valeurs");
                //return;
            }

            var query = Inputs;
            //var query = new List<IDocument>();
            //query =   ((System.Collections.Generic.List<IDocument>) Inputs) ;
            if (GetTypeName(SelectedProeprty.PropertyType) == "DateTime")
            {
                var _Valeur = DateTime.Parse(Valeur);
                switch (ConditionsSelected.Value)
                {


                    case EnumConditions.egale:
                        var result = query.Where(a => (SelectedProeprty.GetValue(a, null) as DateTime?)?.Date == (_Valeur.Date));
                        Result = result.ToList();
                        Inputs = Result;

                        break;
                    case EnumConditions.ressemble:
                        var resultRes = query.Where(a => (SelectedProeprty.GetValue(a, null) as DateTime?)?.Date == (_Valeur.Date));
                        Result = resultRes.ToList();
                        Inputs = Result;

                        break;
                    case EnumConditions.déffirent:
                        var resultDef = query.Where(a => !((SelectedProeprty.GetValue(a, null) as DateTime?)?.Date == _Valeur.Date));
                        Result = resultDef.ToList();
                        Inputs = Result;
                        break;
                    case EnumConditions.inférieur:
                        var resultInf = query.Where(a => (SelectedProeprty.GetValue(a, null) as DateTime?)?.Ticks <= (_Valeur.Ticks));
                        Result = resultInf.ToList();
                        Inputs = Result;
                        break;
                    case EnumConditions.supérieur:
                        var resultSup = query.Where(a => (SelectedProeprty.GetValue(a, null) as DateTime?)?.Ticks >= (_Valeur.Ticks));
                        Result = resultSup.ToList();
                        Inputs = Result;
                        break;

                    default:
                        break;
                }
            }
            if (GetTypeName(SelectedProeprty.PropertyType) == "Int32")
            {

                var _Valeur = int.Parse(Valeur);
                switch (ConditionsSelected.Value)
                {
                    case EnumConditions.ressemble :
                        try
                        {
                            var resultd = query.Where(a => SelectedProeprty.GetValue(a, null).ToString().Contains(Valeur));
                            Result = resultd.ToList();
                            Inputs = Result;
                        }
                        catch
                        { }


                        break;
                    case EnumConditions.egale:
                        var result = query.Where(a => (int?)SelectedProeprty.GetValue(a, null) == (_Valeur));
                        Result = result.ToList();
                        Inputs = Result;

                        break;
                    case EnumConditions.déffirent:
                        var resultDef = query.Where(a => (int?)SelectedProeprty.GetValue(a, null) != _Valeur);
                        Result = resultDef.ToList();
                        Inputs = Result;
                        break;
                    case EnumConditions.inférieur:
                        var resultInf = query.Where(a => (int?)SelectedProeprty.GetValue(a, null) <= (_Valeur));
                        Result = resultInf.ToList();
                        Inputs = Result;
                        break;
                    case EnumConditions.supérieur:
                        var resultSup = query.Where(a => (int?)SelectedProeprty.GetValue(a, null) >= (_Valeur));
                        Result = resultSup.ToList();
                        Inputs = Result;
                        break;

                    default:
                        break;
                }
            }
            if (GetTypeName(SelectedProeprty.PropertyType) == "Decimal")
            {

                var _Valeur = decimal.Parse(Valeur);
                switch (ConditionsSelected.Value)
                {
                    case EnumConditions.ressemble:
                        try
                        {
                            var resultd = query.Where(a => SelectedProeprty.GetValue(a, null)?.ToString()?.Contains(Valeur) == true);
                            Result = resultd.ToList();
                            Inputs = Result;
                        }
                        catch
                        { }

                        break;
                    case EnumConditions.egale:
                        var result = query.Where(a => (decimal?)SelectedProeprty.GetValue(a, null) == (_Valeur));
                        Result = result.ToList();
                        Inputs = Result;

                        break;
                    case EnumConditions.déffirent:
                        var resultDef = query.Where(a => (decimal?)SelectedProeprty.GetValue(a, null) != _Valeur);
                        Result = resultDef.ToList();
                        Inputs = Result;
                        break;
                    case EnumConditions.inférieur:
                        var resultInf = query.Where(a => (decimal?)SelectedProeprty.GetValue(a, null) <= (_Valeur));
                        Result = resultInf.ToList();
                        Inputs = Result;
                        break;
                    case EnumConditions.supérieur:
                        var resultSup = query.Where(a => (decimal?)SelectedProeprty.GetValue(a, null) >= (_Valeur));
                        Result = resultSup.ToList();
                        Inputs = Result;
                        break;

                    default:
                        break;
                }
            }
            if (GetTypeName(SelectedProeprty.PropertyType) == "Double")
            {

                var _Valeur = double.Parse(Valeur);
                switch (ConditionsSelected.Value)
                {
                    case EnumConditions.ressemble:
                        try
                        {
                            var resultd = query.Where(a => SelectedProeprty.GetValue(a, null).ToString().Contains(Valeur));
                            Result = resultd.ToList();
                            Inputs = Result;
                        }
                        catch
                        { }
                        break;
                    case EnumConditions.egale:
                        var result = query.Where(a => (double?)SelectedProeprty.GetValue(a, null) == (_Valeur));
                        Result = result.ToList();
                        Inputs = Result;

                        break;
                    case EnumConditions.déffirent:
                        var resultDef = query.Where(a => (double?)SelectedProeprty.GetValue(a, null) != _Valeur);
                        Result = resultDef.ToList();
                        Inputs = Result;
                        break;
                    case EnumConditions.inférieur:
                        var resultInf = query.Where(a => (double?)SelectedProeprty.GetValue(a, null) <= (_Valeur));
                        Result = resultInf.ToList();
                        Inputs = Result;
                        break;
                    case EnumConditions.supérieur:
                        var resultSup = query.Where(a => (double?)SelectedProeprty.GetValue(a, null) >= (_Valeur));
                        Result = resultSup.ToList();
                        Inputs = Result;
                        break;

                    default:
                        break;
                }
            }
            if (GetTypeName(SelectedProeprty.PropertyType) == "String")
            {
                switch (ConditionsSelected.Value)
                {

                    case EnumConditions.ressemble:
                        try
                        {
                            var results = query.Where(s =>
                            {
                                var value = SelectedProeprty.GetValue(s, null);
                                if (value == null) return false;
                                return value.ToString().ToLower().Contains(Valeur.ToLower());
                            });


                            Result = results.ToList();
                            Inputs = Result;

                        }
                        catch
                        { }

                        break;
                    case EnumConditions.egale:
                        var result = query.Where(s =>
                        {
                            var value = SelectedProeprty.GetValue(s, null);
                            if (value == null) return false;
                            return value.ToString() == (Valeur);
                        });

                        Result = result.ToList();
                        Inputs = Result;

                        break;
                    case EnumConditions.déffirent:
                        var resultDef = query.Where(s =>
                        {
                            var value = SelectedProeprty.GetValue(s, null);
                            if (value == null) return false;
                            return !(value.ToString() == (Valeur));
                        });
                        Result = resultDef.ToList();
                        Inputs = Result;
                        break;


                    default:
                        break;
                }
            }
            if (GetTypeName(SelectedProeprty.PropertyType) == "Guid")
            {
                var attributes = SelectedProeprty.GetCustomAttribute(typeof(ColumnAttribute)) as ColumnAttribute;
                var option = attributes.Options;
                var link = DS.Generic(option)?.Find("Name", Valeur, true)?.FirstOrDefault();
                if (link == null && ConditionsSelected.Value != EnumConditions.ressemble )
                    return;
                switch (ConditionsSelected.Value)
                {


                    case EnumConditions.egale   :
                        var result = query.Where(a => SelectedProeprty.GetValue(a, null).ToString().Equals(link.Id.ToString()));
                        Result = result.ToList();
                        Inputs = Result;

                        break;
                    case EnumConditions.ressemble:
                        var resultRes = query.Where(a => SelectedProeprty.GetValue(a, null).ToString().Equals(link.Id.ToString()));
                        Result = resultRes.ToList();
                        Inputs = Result;

                        break;
                    case EnumConditions.déffirent:
                        var resultDef = query.Where(a => !SelectedProeprty.GetValue(a, null).ToString().Contains(link.Id.ToString()));
                        Result = resultDef.ToList();
                        Inputs = Result;
                        break;


                    default:
                        break;
                }
            }
            if (GetTypeName(SelectedProeprty.PropertyType) == "Boolean")
            {
                switch (ConditionsSelected.Value)
                {

                    case EnumConditions.ressemble:
                        try
                        {
                            var results = query.Where(s =>
                            {
                                var value = SelectedProeprty.GetValue(s, null);
                                if (value == null) return false;
                                return value.ToString().Contains(Valeur);
                            });


                            Result = results.ToList();
                            Inputs = Result;

                        }
                        catch
                        { }

                        break;
                    case EnumConditions.egale:
                        var result = query.Where(s =>
                        {
                            var value = SelectedProeprty.GetValue(s, null);
                            if (value == null) return false;
                            return value.ToString() == (Valeur);
                        });

                        Result = result.ToList();
                        Inputs = Result;

                        break;
                    case EnumConditions.déffirent:
                        var resultDef = query.Where(s =>
                        {
                            var value = SelectedProeprty.GetValue(s, null);
                            if (value == null) return false;
                            return !(value.ToString() == (Valeur));
                        });
                        Result = resultDef.ToList();
                        Inputs = Result;
                        break;


                    default:
                        break;
                }
            }

        }

        public static string GetTypeName(Type type)
        {
            var nullableType = Nullable.GetUnderlyingType(type);

            bool isNullableType = nullableType != null;

            if (isNullableType)
                return nullableType.Name;
            else
                return type.Name;
        }

        public void doAnnuler()
        {
            this.RequestClose();
        }

        public void Dispose()
        {
        }

        public void showAll()
        {
            Result = Inputs;
            this.RequestClose(true);
        }
    }
}
