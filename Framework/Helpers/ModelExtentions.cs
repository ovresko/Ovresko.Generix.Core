using Ovresko.Generix.Core.Modules.Core.Data;
using Ovresko.Generix.Core.Modules.Core.Module;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ovresko.Generix.Datasource.Models;

namespace Ovresko.Generix.Core.Modules.Core.Helpers
{
    public static class ModelExtentions
    {
        public static bool IsValide(this Guid id)
        {
                return (id != Guid.Empty && id != null);
          
        }


        //public static bool IsValide(this Guid id)
        //{
        //    if (id.HasValue)
        //        return (id != Guid.Empty && id != null);
        //    return false;
        //}

        public static dynamic GetObject(this Guid id, string model)
        { 
            return DS.Generic(model)?.GetById(id);
         
        }
        //public static dynamic GetObject(this Guid id, string model)
        //{
        //    if(id.HasValue)
        //        return DS.Generic(model).GetById(id);
        //    return null;
        //}
        public static string GetName<T>(this Guid id) where T : IDocument
        { 
            return DS.db.GetById<T>(id)?.NameSearch;
          
        }


        public static T GetObject<T>(this Guid id) where T:IDocument
        {
            
                return DS.db.GetById<T>(id);
           
        }

        public static bool IsValideAndSubmited<T>(this Guid id) where T : IDocument
        {
            if (id == null)
                return false;

            if (id.IsValide() && id.GetObject<T>()?.DocStatus == 1) return true;
                return false;
        }

        
    }
}
