using Ovresko.Generix.Core.Modules.Core;
using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Core.Modules.Core.Helpers;
using Ovresko.Generix.Core.Modules.Core.Module;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ovresko.Generix.Core.Modules.Core.Helpers;
using static Ovresko.Generix.Core.Translations.OvTranslate;
using Stylet;
using System.IO;
using Microsoft.Win32;
using Newtonsoft.Json;
using Ovresko.Generix.Core.Exceptions;
using Ovresko.Generix.Datasource.Models;

namespace Ovresko.Generix.Core.Modules
{
    public class ModuleErp : ModelBase<ModuleErp>
    {

        #region SETTINGS

        public override bool Submitable { get; set; } = false;
        public override string ModuleName { get; set; } = "Systémes";
        public override string CollectionName { get; } = _("Module");
        public override OpenMode DocOpenMod { get; set; } = OpenMode.Detach;
        public override string IconName { get; set; } = "Gears";
        public override bool ShowInDesktop { get; set; } = false;

        public override string NameField { get; set; } = "Libelle";

        #endregion

        public bool HasSeries { get; set; }

        public override void Validate()
        {
            base.Validate();
            base.ValidateUnique();
        }
        public ModuleErp()
        {
        }


        [ColumnAttribute(ModelFieldType.ReadOnly, "")]
        [IsBoldAttribute(true)]
        [ShowInTable(true)]
        [ExDisplayName("Libellé")]
        public string Libelle { get; set; }

        [ColumnAttribute(ModelFieldType.ReadOnly, "")]
        [IsBoldAttribute(false)]
        [ShowInTable(true)]
        [ExDisplayName("Lien Classe")]
        public string ClassName { get; set; }


        [ExDisplayName("Icon d'affichage")]
        [Column(ModelFieldType.Text, "")]
        public string ModuleIcon { get; set; } = "ChevronRight";


        [Column(ModelFieldType.Check, "Singltone?")]
        [ExDisplayName("Une instance")]
        public bool IsInstanceModule { get; set; }



        [Column(ModelFieldType.Check, "Peut valider?")]
        [ExDisplayName("Validable")]
        public bool ModuleSubmitable { get; set; }


        [ColumnAttribute(ModelFieldType.ReadOnly, "")]
        [ExDisplayName("Instance function")]
        public string InstanceFunction { get; set; } = "getInstance";

        [ColumnAttribute(ModelFieldType.Check, "Afficher sur le bureau")]
        [IsBoldAttribute(false)]
        [ShowInTable(false)]
        [ExDisplayName("Accée rapide")]
        public bool EstAcceRapide { get; set; }

        [ColumnAttribute(ModelFieldType.Check, "Afficher sur le menu")]
        [ExDisplayName("Accée Menu")]
        public bool EstTopBar { get; set; }

        [Column(ModelFieldType.Numero, "")]
        [ExDisplayName("Position Menu")]
        public int ModuleMenuIndex { get; set; }

        [ColumnAttribute(ModelFieldType.Separation, "")]
        [BsonIgnore]
        [ExDisplayName("Groupe module")]
        public string Grp { get; set; }


        [ColumnAttribute(ModelFieldType.Text, "")]
        [ShowInTable(true)]
        [ExDisplayName("Libellé de groupe module")]
        public string GroupeModule { get; set; }


        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("Champ d'identification")]
        public string NameFieldEntity { get; set; }

        [SetColumn(2)]
        [ExDisplayName("Mettre à jour")]
        [Column(ModelFieldType.Button, "UpdateDataRefs")]
        public string UpdateDataRefsBtn { get; set; }

        public void UpdateDataRefs()
        {
            try
            {
                Type t = DataHelpers.GetTypesModule.Resolve(ClassName);


                var collection = t;// Type.GetType(ClassName);
                var items = DS.Generic(collection)?.GetAll(); // as IEnumerable<IDocument>;
                if (items != null)
                {
                    foreach (IDocument item in items)
                    {
                        item.ForceIgniorValidatUnique = true;
                        if (!(item as IModel).Save())
                            return;
                    }

                    DataHelpers.ShowMessage("Documents à jour!");
                }
            }
            catch (Exception s)
            {
                DataHelpers.ShowMessageError(s);
            }
        }
        //[ShowInTableAttribute(false)]
        //[ExDisplayName("Série par default")]
        //[ColumnAttribute(ModelFieldType.Lien, "SeriesName")]
        //public Guid SeriesDefault { get; set; } = Guid.Empty;

        [ExDisplayName("Tempaltes d'impression")]
        [Column(ModelFieldType.Button, "OpenPrint")]
        public string OpenPrintBtn { get; set; }

        public void OpenPrint()
        {
            try
            {
                Type type = DataHelpers.GetTypesModule.Resolve(ClassName);


                var instance = Activator.CreateInstance(type);
                (instance as IDocument).ExportWORD(type);
            }
            catch (Exception s)
            {
                DataHelpers.ShowMessageError(s);
            }
        }


        [ExDisplayName("Effacer tout les données")]
        [Column(ModelFieldType.Button, "DeleteAll")]
        public string DeleteAllBtn { get; set; }

        public void DeleteAll()
        {
            var res =  DataHelpers.ShowMessage("Effacer tout ?", "", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
               

                try
                {
                    Type cls = DataHelpers.GetTypesModule.Resolve(this.ClassName);
             
                      DS.Generic(cls)?.DropCollection();// DataHelpers.GetGenericData(cls) ;
                    //var metGetCount = (generic.GetType() as Type).GetMethod("Clear");
                    // metGetCount.Invoke(generic, null);
                    DataHelpers.ShowMessage("Terminé");
                }
                catch (Exception s)
                {
                     DataHelpers.ShowMessage(s.Message);
                }
            }
        }




        [ExDisplayName("Série par default")]
        [ColumnAttribute(ModelFieldType.LienField, "SeriesNames")]
        public Guid SeriesDefault { get; set; }

        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("Modéle d'impression par Default")]
        public string DefaultTemplateName { get; set; }

        [ColumnAttribute(ModelFieldType.Separation, "")]
        [BsonIgnore]
        [ExDisplayName("Séries")]
        public string sepSeries { get; set; }

        [ColumnAttribute(ModelFieldType.Table, "SeriesName")]
        [ShowInTable(false)]
        [ExDisplayName("Séries disponible")]
        [myTypeAttribute(typeof(SeriesName))]
        public List<SeriesName> SeriesNames { get; set; } = new List<SeriesName>();

        [Column(ModelFieldType.BaseButton, "AddModules")]
        [ExDisplayName("Ajt. module")]
        public string AddModulesAction { get; set; }

        public static bool PluginIsValide(string name,string user, string key)
        {

            return true;
        }


        public static void AddModules()
        {
            try
            {
                OpenFileDialog add = new OpenFileDialog();
                var res = add.ShowDialog();
                add.DefaultExt = "dll";
                if (res == true)
                {
                    var fullpath =Path.GetFullPath(add.FileName);                    
                    FrameworkManager.InstallPlugin(fullpath, add.SafeFileName,FrameworkManager.InstallMode.Path);
                }
            }
            catch (Exception s)
            {
                 DataHelpers.ShowMessage(s.Message);
                return;
            }
        }

        [Column(ModelFieldType.BaseButton, "ReloadModules")]
        [ExDisplayName("Rech. modules")]
        public string ReloadModulesAction { get; set; }

        [Column(ModelFieldType.BaseButton, "ReloadSeries")]
        [ExDisplayName("Rech. séries")]
        public string ReloadSeriesAction { get; set; }

        public void ReloadModules()
        {
            FrameworkManager.UpdateModules(true);
            DataHelpers.Shell.SetupSideMenu().Wait();
        }

        public void ReloadSeries()
        {
            try
            {
                var res =  DataHelpers.ShowMessage("Voulez-vous effacer les anciennes séries?", "Confirmation", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.Yes)
                {
                    FrameworkManager.CreateSeries(true);
                }
                else
                {
                    FrameworkManager.CreateSeries(false);
                }

                 DataHelpers.ShowMessage("Crétion des séries par default terminé.");
            }
            catch (Exception s)
            {
                 DataHelpers.ShowMessage(s.Message);
            }
        }


        public override bool Save()
        {
            var result = base.Save();
            if (result)
            {

                // update loaded modules in DataHelpers
                var modules = DS.db.GetAll<ModuleErp>();// DataHelpers.GetMongoDataSync("ModuleErp");
                DataHelpers.Modules = modules as IEnumerable<ModuleErp>;

                try
                {
                    Execute.OnUIThread(() =>
                    {
                        DataHelpers.Shell?.SetupTopBar();
                    });
                }
                catch { }

            }
            return result;

        }
    }
}