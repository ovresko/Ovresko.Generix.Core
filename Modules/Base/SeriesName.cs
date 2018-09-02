
// Auto generated class

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
   public class SeriesName : ModelBase<SeriesName>
    {

        #region SETTINGS

        public override bool Submitable { get; set; } = false;
        public override string ModuleName { get; set; } = "Systémes";
        public override string CollectionName { get; } = _("Nom de série");
        public override OpenMode DocOpenMod { get; set; } = OpenMode.Detach;
        public override string IconName { get; set; } = "Altimeter";
        public override bool ShowInDesktop { get; set; } = false;

        public override string NameField { get; set; } = "Libelle";

        #endregion
         
          
        public override void Validate()
        {
            base.Validate();
            base.ValidateUnique();

        }

         


        public SeriesName()
        {

        } 
        

        [IsBoldAttribute(true)]
        [ShowInTable(true)]
        [ColumnAttribute(ModelFieldType.Text, "")]
        [ExDisplayName("Libellé")]
        public string Libelle { get; set; }
        

        [IsBoldAttribute(false)]
        [ShowInTable(true)]
        [ColumnAttribute(ModelFieldType.Text, "")]
        [LongDescription("MM: mois - AAAA: Année ex:2018 - AA: année ex:18 - JJ:jours")]
        [ExDisplayName("Suffix")]
        public string Sufix { get; set; }

        [ColumnAttribute(ModelFieldType.Numero, "")]
        [ShowInTable(true)]
        [ExDisplayName("Indexe")]
        public long Indexe { get; set; }
    }


}