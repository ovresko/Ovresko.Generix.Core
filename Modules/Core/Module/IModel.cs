using MongoDB.Bson.Serialization.Attributes;
using Ovresko.Generix.Datasource.Models;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ovresko.Generix.Core.Modules.Core.Module
{
     
    public interface IModel  
    {
        bool Save();
        bool Delete(bool ConfirmFromUser = true);
        bool Submit();
        bool Cancel();
        Task NotifyUpdates();
        void NotifyUpdates(string source);
        void Open(OpenMode mode);

        // bool DoRefresh { get; set; }

    }
}
