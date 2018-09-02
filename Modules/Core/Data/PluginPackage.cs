using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Core.Modules.Core.Helpers;
using Ovresko.Generix.Core.Modules.Core.Module;
using Ovresko.Generix.Core.Pages.ModulesManager;
using Ovresko.Generix.Datasource.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ovresko.Generix.Core.Translations.OvTranslate;

namespace Ovresko.Generix.Core.Modules
{
    public class PluginPackage : ModelBase<PluginPackage>
    {


        #region SETTINGS

        public override bool Submitable { get; set; } = false;
        public override string ModuleName { get; set; } = "Application";
        public override string CollectionName { get; } = _("Plugin");
        public override OpenMode DocOpenMod { get; set; } = OpenMode.Attach;
        public override string IconName { get; set; } = "PackageVariant";
        public override bool ShowInDesktop { get; set; } = false;

        public override string NameField { get; set; } = "Libelle";

        #endregion




        public override void Validate()
        {
            base.Validate(); 
        }
         

        [Required]
        [ColumnAttribute(ModelFieldType.Text, "")]
        [IsBoldAttribute(true)] 
        [ExDisplayName("Nom plugin")]
        public string Libelle { get; set; }

        [ColumnAttribute(ModelFieldType.Text, "")] 
        [ExDisplayName("Version")]
        public string PluginVersion { get; set; }

        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("Lien")]
        public string PluginLien { get; set; }

        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("Date")]
        public string PluginDate { get; set; }


        [ExDisplayName("Fichiers")]
        [Column(ModelFieldType.Separation,"")]
        public int sep { get;  }


        [ExDisplayName("Fichiers")]
        [Column(ModelFieldType.Table, "PluginFile")]
        [myType(typeof(PluginFile))]
        public List<PluginFile> AllPluginFiles { get; set; } = new List<PluginFile>();

        [ExDisplayName("Installer nouveau")]
        [Column(ModelFieldType.BaseButton,"InstallPlugin")]
        public int InstallPluginBtn { get; set; }

        public void InstallPlugin()
        {
            var modules = new ModulesManagerViewModel(DataHelpers.windowManager);
            DataHelpers.windowManager.ShowWindow(modules);
        }
    }

    public class PluginFile
    {

        [ShowInTable]
        [ExDisplayName("Position fichier")]
        public string PathFile { get; set; }

        public PluginFile(string _PathFile)
        {
            PathFile = _PathFile;
        }

        public PluginFile()
        {

        }
    }
}
