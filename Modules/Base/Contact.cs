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
using System.ComponentModel.DataAnnotations;
using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Datasource.Models;

namespace Ovresko.Generix.Core.Modules
{
   public class Contact : ModelBase<Contact>
    {


        #region SETTINGS

        public override bool Submitable { get; set; } = false;
        public override string ModuleName { get; set; } = "Application";
        public override string CollectionName { get; } = _("Contact");
        public override OpenMode DocOpenMod { get; set; } = OpenMode.Attach;
        public override string IconName { get; set; } = "AccountMultiple";
        public override bool ShowInDesktop { get; set; } = false;

        public override string NameField { get; set; } = "NomComplet";

        #endregion


        public override Dictionary<string, string> InfoCards
        {
            get
            {
                return new Dictionary<string, string>() { { "Nom", this.NomComplet } };
            }
        }

        public override void Validate()
        {
            base.Validate();
          //  base.ValidateUnique();

         

        }
      

        public Contact()
        {
         
        } 

        [Required]
        [ColumnAttribute(ModelFieldType.Text, "")]
        [IsBoldAttribute(true)]
        [ShowInTable(true)]
        [ExDisplayName("Nom et prénom")]
        public string NomComplet { get; set; }


        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("Adresse")]
        public string Adresse { get; set; }

        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("Région")]
        public string Region { get; set; }

        [ShowInTable(true)]
        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("Mobile")]
        public string TelMob { get; set; }

        [ShowInTable(true)]
        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("Téléphone")]
        public string TelFix { get; set; }

        [ShowInTable(true)]
        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("Fax")]
        public string TelFax { get; set; }

        [ShowInTable(true)]
        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("Email")]
        public string Email { get; set; }

        [ShowInTable(true)]
        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("Site web")]
        public string Siteweb { get; set; }

        [ColumnAttribute(ModelFieldType.Separation, "")]
        [BsonIgnore]
        [ExDisplayName("Infos supplémentaires")]
        public string sepcompte { get; set; }

        [ColumnAttribute(ModelFieldType.Select, "Sexe")]
        [ExDisplayName("Sexe")]
        public string SexeClient { get; set; }

        [ColumnAttribute(ModelFieldType.Date, "")]
        [ExDisplayName("Date naissance")]
        public DateTime? Birthday { get; set; }

        [ShowInTable(true)]
        [ColumnAttribute(ModelFieldType.ReadOnly, "")]
        [ExDisplayName("Age")]
        public string Age { get {
                if (Birthday.HasValue)
                    return  DataHelpers.GetPeriodeString(Birthday.Value, DateTime.Now);
                return "N/A";
            } }
        //[ShowInTableAttribute(true)]
        //[IsSourceAttribute("DomaineActivite")]
        //[ExDisplayName("Domaine d'activité")]
        //[ColumnAttribute(ModelFieldType.Lien, "DomaineActivite")]
        //public Guid lDomaineActivite { get; set; } = Guid.Empty;

    }
}
