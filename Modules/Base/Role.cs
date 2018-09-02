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
using Ovresko.Generix.Core.Modules.Core.Helpers;
using static Ovresko.Generix.Core.Translations.OvTranslate;
using Ovresko.Generix.Datasource.Models;

namespace Ovresko.Generix.Core.Modules
{
    public class Role : ModelBase<Role>
    {
        #region SETTINGS

        public override bool Submitable { get; set; } = false;
        public override string ModuleName { get; set; } = "Systémes";
        public override string CollectionName { get; } = _("Rôle");
        public override OpenMode DocOpenMod { get; set; } = OpenMode.Attach;
        public override string IconName { get; set; } = "AccountMultiple";
        public override bool ShowInDesktop { get; set; } = false;

        #endregion

        public override string NameField { get; set; } = "Libelle";


        public override void Validate()
        {
            base.Validate();
            base.ValidateUnique();
        }
        public Role()
        {
        } 
      



        [ColumnAttribute(ModelFieldType.Text, "")]
        [IsBoldAttribute(true)]
        [ShowInTable(true)]
        [ExDisplayName("Libellé")]
        public string Libelle { get; set; }


        [ColumnAttribute(ModelFieldType.Separation, "")]
        [BsonIgnore]
        [ExDisplayName("Règles d'autorisation")]
        public string sepRegles { get; set; }

        [ColumnAttribute(ModelFieldType.Table, "AccesRule")]
        [ShowInTable(false)]
        [ExDisplayName("Règles d'autorisation")]
        [myTypeAttribute(typeof(AccesRule))]
        public List<AccesRule> AccesRules { get; set; } = new List<AccesRule>();


    }
}