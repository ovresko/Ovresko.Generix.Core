using Ovresko.Generix.Datasource.Queries;
using Ovresko.Generix.Datasource.Services;
using Ovresko.Generix.Datasource.Services.Queries;
using Ovresko.Generix.Utils.Data.Sources;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ovresko.Generix.Core.Modules.Core.Data
{
    public class DS
    { 

        public static IGenericQuery Generic(Type type)
        {
            if (type == null)
                throw new Exception("Type is null");

            return DS.db.Generic(type);
        }
        public static IGenericQuery Generic(string type)
        {
             
                var resolvedType = DataHelpers.GetTypesModule.Resolve(type);
                return DS.db.Generic(resolvedType);
            
        }


        public static IQuery _db = null; 
        public static IQuery db
        {
            get
            {
              
                if (_db == null)
                {
                    IDataService service = DataHelpers.container.Get<IDataService>();
                    return _db = service.GetQuery();
                    //var adr = Properties.Settings.Default.MongoServerSettings;
                    //var dbName = Properties.Settings.Default.dbUrl;

                    //_db = new DataSource(dbName, adr);
                }

                return _db;
            }
        }

    }
}
