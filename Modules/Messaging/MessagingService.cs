using Ovresko.Generix.Core.Modules.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ovresko.Generix.Core.Translations.OvTranslate;

namespace Ovresko.Generix.Core.Modules.Messaging
{
    public class MessagingService : IMessagingService
    {
        private void Validate(CoreMessageBase messageBase)
        {
            if (string.IsNullOrWhiteSpace(messageBase.Message))
                throw new Exception("Message est vide");
            
        }

        public CoreMessageBase CreateMessage(DateTime due, string message, string docName, string Status = "En attente")
        {
            CoreMessageBase messageBase = new CoreMessageBase();
            messageBase.Message = message;
            messageBase.DueDate = due;
            messageBase.StatusMessage = Status;
            messageBase.TargetName = docName;
            Validate(messageBase);

            DataHelpers.Shell.UpdateNotificationsTitle( );
            if (messageBase.Save())
                return messageBase;
            return null;
        }

        public CoreMessageBase CreateMessage(int afterDays , string message, string docName, string Status = "En attente")
        {
            CoreMessageBase messageBase = new CoreMessageBase();
            messageBase.Message = message;
            messageBase.DueDate = DateTime.Today.AddDays(afterDays  );
            messageBase.StatusMessage = Status;
            messageBase.TargetName = docName;
            Validate(messageBase);
            DataHelpers.Shell.UpdateNotificationsTitle();

            if (messageBase.Save())
                return messageBase;
            return null;
        }

        public IEnumerable<CoreMessageBase> GetMessaesTermine()
        {
            return DS.db.GetAll<CoreMessageBase>(a => a.StatusMessage == _("Vue / terminée"));
        }

        public IEnumerable<CoreMessageBase> GetMessagesEnAttent()
        {
            return DS.db.GetAll<CoreMessageBase>(a => a.StatusMessage != _("Vue / terminée"));
        }
    }
}
