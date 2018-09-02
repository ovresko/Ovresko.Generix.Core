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
    public class AccesRule : ModelBase<AccesRule>
    {

        #region SETTINGS

        public override bool Submitable { get; set; } = false;
        public override string ModuleName { get; set; } = "Systémes";
        public override string CollectionName { get; } = _("Règles d'autorisation");
        public override OpenMode DocOpenMod { get; set; } = OpenMode.Detach;
        public override string IconName { get; set; } = "LockOpenOutline";
        public override bool ShowInDesktop { get; set; } = false;

        #endregion

        #region NAMING

        public override string Name { get { return Naming(); } set => base.Name = value; }
         
        #endregion
         

        public override void Validate()
        {
            base.Validate();
            //base.ValidateUnique();
        }
        public AccesRule()
        {
        }
       
         
        [ShowInTableAttribute(false)]
        [ExDisplayName("Module")]
        [ColumnAttribute(ModelFieldType.Lien, "ModuleErp")]
        [IsSourceAttribute("ModuleErp")]
        public Guid Module { get; set; } = Guid.Empty;

        [SetColumn(2)]
        [ShowInTable()]
        [ColumnAttribute(ModelFieldType.Check, "Peut voir ?")]
        [ExDisplayName("Voir")]
        public bool Voir { get; set; } = true;

        [SetColumn(2)]
        [ShowInTable()]
        [ColumnAttribute(ModelFieldType.Check, "Peut Créer ?")]
        [ExDisplayName("Créer")]
        public bool Creer { get; set; } = true;

        [SetColumn(2)]
        [ShowInTable()]
        [ColumnAttribute(ModelFieldType.Check, "Peut modifier ?")]
        [ExDisplayName("Modifier")]
        public bool CanSave { get; set; } = true;

        [ColumnAttribute(ModelFieldType.Check, "Peut Supprimer ?")]
        [SetColumn(2)]
        [ExDisplayName("Supprimer")]
        [ShowInTable()]
        public bool Supprimer { get; set; }

        [SetColumn(2)]
        [ColumnAttribute(ModelFieldType.Check, "Peut valider ?")]
        [ExDisplayName("Valider")]
        [ShowInTable()]
        public bool Valider { get; set; }

        [SetColumn(2)]
        [ColumnAttribute(ModelFieldType.Check, "Peut Annuler ?")]
        [ExDisplayName("Annuler")]
        [ShowInTable()]
        public bool CancelSubmit { get; set; }

        private string Naming()
        {
            var voir = Voir ? _("Voir") : "";
            var valider = Valider ? _("Valider") : "";
            var supp = Supprimer ? _("Supprimer") : "";
            var cree = Creer ? _("Créer") : "";
            return $"{Module.GetObject("ModuleErp")?.Name} ({voir} {valider} {supp} {cree})";
        }

    }
}