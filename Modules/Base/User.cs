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
using static Ovresko.Generix.Core.Translations.OvTranslate;
using System.Threading.Tasks;
using Ovresko.Generix.Core.Modules.Core.Helpers;
using Ovresko.Generix.Datasource.Models;

namespace Ovresko.Generix.Core.Modules
{
    public class User : ModelBase<User>
    {
        #region SETTINGS

        public override bool Submitable { get; set; } = false;
        public override string ModuleName { get; set; } = "Systémes";
        public override string CollectionName { get; } = _("Utilisateur");
        public override OpenMode DocOpenMod { get; set; } = OpenMode.Attach;
        public override string IconName { get; set; } = "AccountKey";
        public override bool ShowInDesktop { get; set; } = false;

        public override string NameField { get; set; } = "Libelle";

        #endregion

         
        public override void Validate()
        {
            base.Validate();
            base.ValidateUnique();
        }
        public User()
        {
        }
        
        [ColumnAttribute(ModelFieldType.Text, "")]
        [IsBoldAttribute(true)]
        [ShowInTable(true)]
        [ExDisplayName("Nom et prénom")]
        public string Libelle { get; set; }

        [ColumnAttribute(ModelFieldType.Text, "")] 
        [ExDisplayName("Fonction")]
        public string Fonction { get; set; }

        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("E-Mail")]
        public string Email { get; set; }

       


        [ColumnAttribute(ModelFieldType.Separation, "")]
        [BsonIgnore]
        [ExDisplayName("Rôles")]
        public string sepRoles { get; set; }

        [ColumnAttribute(ModelFieldType.WeakTable, "Role")]
        [ExDisplayName("Rôles")]
        public List<Guid> Roles { get; set; } = new List<Guid>();

        [ColumnAttribute(ModelFieldType.Separation, "")]
        [BsonIgnore]
        [ExDisplayName("Autorisations")]
        public string seppermissions { get; set; }

        [ColumnAttribute(ModelFieldType.Check, "Est administrateur?")]
        [ExDisplayName("Admin systéme")]
        public bool IsAdmin { get; set; }

        [ColumnAttribute(ModelFieldType.Text, "")]
        [IsBoldAttribute(false)]
        [ShowInTable(false)]
        [ExDisplayName("Mots de passe")]
        public string Password { get; set; }

    }
}