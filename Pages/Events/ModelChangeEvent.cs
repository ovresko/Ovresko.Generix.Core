using MongoDB.Bson;
using Ovresko.Generix.Core.Modules.Core.Module;
using Ovresko.Generix.Datasource.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ovresko.Generix.Core.Pages.Events
{
   public class ModelChangeEvent
    {
        public ModelChangeEvent(Type type)
        {
            this.type = type;
        }

        public Type type { get; set; }

    }

    public class DetailModelChangeEvent
    {
        public DetailModelChangeEvent(IDocument doc, Type type, Guid id)
        {
            this.type = type;
            this.doc = doc;
            this.id = id;
        }
        public IDocument doc { get; set; }
        public Type type { get; set; }
        public Guid id { get; set; }

    }

}
