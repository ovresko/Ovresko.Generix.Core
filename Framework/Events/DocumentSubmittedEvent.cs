using Ovresko.Generix.Datasource.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ovresko.Generix.Core.Framework.Events
{
    public class DocumentSubmittedEvent
    {
        public DocumentSubmittedEvent(IDocument eventDocument, string eventName)
        {
            EventDocument = eventDocument;
            EventName = eventName;
        }

        public IDocument EventDocument { get; set; }
        public string EventName { get; set; }

    }
}
