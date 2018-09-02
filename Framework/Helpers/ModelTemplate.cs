using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ovresko.Generix.Core.Modules.Core.Module;
using Ovresko.Generix.Core.Modules.Core.Helpers;

using static Ovresko.Generix.Core.Translations.OvTranslate;
namespace Ovresko.Generix.Core.Modules.Core.Helpers
{
    public class ModelTemplate : ModelBase<ModelTemplate>
    {
        
        [BsonIgnore]
        public override string SubModule { get; set; } = _("Articles et prix");
        [BsonIgnore]
        public override string IconName { get; set; } = "Gears";

        [BsonIgnore]
        public override string ModuleName { get; set; } = "Produits";
        public ModelTemplate()
        {

        }
        public override string CollectionName { get; } = _("ModelTemplate");



        #region PROPERTIES
        [ColumnAttribute(ModelFieldType.Text, "")]
        [IsBoldAttribute(true)]
        [ShowInTableAttribute(true)]
        [ExDisplayName("Nom")]
        public string Nom { get; set; }
        public override string Name
        {
            get
            {
                return Nom;
            }
        }

       #endregion



    }
}
