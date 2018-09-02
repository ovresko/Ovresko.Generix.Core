using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ovresko.Generix.Core.Modules.Messaging
{
   public interface IMessagingService
    {
        CoreMessageBase CreateMessage(DateTime due, string message,string docName, string Status = "En attente");
        CoreMessageBase CreateMessage(int afterDays, string message, string docName, string Status = "En attente");
        IEnumerable<CoreMessageBase> GetMessagesEnAttent();
        IEnumerable<CoreMessageBase> GetMessaesTermine();

    }
}
