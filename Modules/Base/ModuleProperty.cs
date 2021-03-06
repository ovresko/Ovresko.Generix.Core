﻿using Ovresko.Generix.Core.Modules.Core.Helpers;
using Ovresko.Generix.Core.Modules.Core.Module;
using Ovresko.Generix.Datasource.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ovresko.Generix.Core.Translations.OvTranslate;

namespace Ovresko.Generix.Core.Modules
{
    class ModuleProperty : ModelBase<ModuleProperty> ,NoModule
    {


        #region SETTINGS

        public override bool Submitable { get; set; } = false;
        public override string ModuleName { get; set; } = "Application";
        public override string CollectionName { get; } = _("Champs");
        public override OpenMode DocOpenMod { get; set; } = OpenMode.Detach;
        public override string IconName { get; set; } = "Settings";
        public override bool ShowInDesktop { get; set; } = false;

        public override string NameField { get; set; } = "_ProeprtyName";

        #endregion

        public override void Validate()
        {
            base.Validate();
     
        }




        [ColumnAttribute(ModelFieldType.Select, "TypeColumn")]
        [ExDisplayName("Type Column")]
        public string _TypeComlumn { get; set; }


        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("Property name")]
        public string _ProeprtyName { get; set; }
        
        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("Display option")]
        public string _DisplayOption { get; set; }

        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("Valeur initial")]
        public string _InitValue { get; set; }


        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("Description")]
        public string _LongDescrption { get; set; }

        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("Display name")]
        public string _DisplayName { get; set; }


        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("Type (string, decimal...)")]
        public string _ThisPropertyType { get; set; }

        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("My Type")]
        public string _TypeProperty { get; set; }


        [ColumnAttribute(ModelFieldType.Check, "")]
        [ExDisplayName("_IsBold")]
        public bool _IsBold { get; set; }


        [ColumnAttribute(ModelFieldType.Check, "")]
        [ExDisplayName("_ShowInTable")]
        public bool _ShowInTable { get; set; }

        [ColumnAttribute(ModelFieldType.Check, "")]
        [ExDisplayName("_DontShowDetail")]
        public bool _DontShowDetail { get; set; }


        [ColumnAttribute(ModelFieldType.Check, "")]
        [ExDisplayName("_IsRequred")]
        public bool _IsRequred { get; set; }



        [ColumnAttribute(ModelFieldType.Check, "")]
        [ExDisplayName("_IsIgnior")]
        public bool _IsIgnior { get; set; }

    }
}
