using MongoDB.Bson;
using Ovresko.Generix.Core.Modules.Core.Data;
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
   public class CoreMessageBase : ModelBase<CoreMessageBase>
    {
        #region SETTINGS

        public override bool Submitable { get; set; } = false;
        public override string ModuleName { get; set; } = "Application";
        public override string CollectionName { get; } = _("Notifications");
        public override OpenMode DocOpenMod { get; set; } = OpenMode.Detach;
        public override string IconName { get; set; } = "BellRingOutline";
        public override bool ShowInDesktop { get; set; } = true;
        public override string NameField { get; set; } = "Message";

        #endregion


        public CoreMessageBase()
        {

        }

        public static void OpenListMessages()
        {
            var msgs = DS.db.GetAll<CoreMessageBase>(a => a.DueDate <= DateTime.Now);
            DataHelpers.Shell.OpenScreenFindAttach(typeof(CoreMessageBase), "Notifications", msgs);
        }
        public override string Status => this.StatusMessage;

        [ShowInTable]
        [ExDisplayName("Date")]
        [Column(ModelFieldType.Date,"")]
        public DateTime? DueDate { get; set; }

        
        [ExDisplayName("Status")]
        [Column(ModelFieldType.Select, "MessageStatus")]
        public string StatusMessage { get; set; } = _("En attente");

        [ExDisplayName("Message")]
        [Column(ModelFieldType.TextLarge,"")]
        public string Message { get; set; }

        [ExDisplayName("Document")]
        [Column(ModelFieldType.Separation,"")]
        public int sepration { get;  }

        [ShowInTable]
        [ExDisplayName("Document")]
        [Column(ModelFieldType.ReadOnly,"")]
        public string TargetName { get; set; }


        public override bool Save()
        {
            var s= base.Save();
            DataHelpers.Shell.UpdateNotificationsTitle();
            return s;
        }

        public override void BeforeClose()
        {
            
            if (!this.isLocal && this.StatusMessage == _("En attente"))
            {
                this.StatusMessage = _("Vue / terminée");
                this.Save();
            }
            DataHelpers.Shell.UpdateNotificationsTitle();

            base.BeforeClose();
        }
       

    }
}
