using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Ovresko.Generix.Core.Translations.OvTranslate;
using System.Threading.Tasks;

namespace Ovresko.Generix.Core.Modules.Messaging
{
   public interface IMessagingService
    {
        CoreMessageBase CreateMessage(DateTime due, string message,string docName, string Status);
        CoreMessageBase CreateMessage(int afterDays, string message, string docName, string Status);
        IEnumerable<CoreMessageBase> GetMessagesEnAttent();
        IEnumerable<CoreMessageBase> GetMessaesTermine();

    }
}
