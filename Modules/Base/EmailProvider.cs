using Ovresko.Generix.Core.Modules.Core.Helpers;
using Ovresko.Generix.Core.Modules.Core.Module;
using Ovresko.Generix.Datasource.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ovresko.Generix.Core.Translations.OvTranslate;

namespace Ovresko.Generix.Core.Modules
{
    public class EmailProvider : ModelBase<EmailProvider>
    {
        public override string CollectionName { get; } = _("Email provider");
        public override string ModuleName { get; set; } = "Systémes";
        public override string IconName { get; set; } = "EmailOpenOutline";
        public override OpenMode DocOpenMod { get; set; } = OpenMode.Detach;
        public override string NameField { get; set; } = "ProviderName";

        public override void Validate()
        {
            base.Validate();
            base.ValidateUnique();
        }

        [ExDisplayName("Provider Name")]
        [Column(ModelFieldType.Text, "")]
        public string ProviderName { get; set; }

        [ExDisplayName("IP Serveur d'accées")]
        [Column(ModelFieldType.Text, "")]
        public string HostIp { get; set; }

        [ExDisplayName("Numéro Port")]
        [Column(ModelFieldType.Numero, "")]
        public int PortNumber { get; set; }

    }
}
