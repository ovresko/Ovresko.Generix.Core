using MongoDB.Bson;
using MongoDB.Driver;
using Ovresko.Generix.Core.Modules.Core.Module;
using Ovresko.Generix.Datasource.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows;

namespace Ovresko.Generix.Core.Modules.Core.Data
{
    public class GenericData<T> where T : IDocument, new()
    {
        //public GenericData()
        //{
        //}

        //public void Clear()
        //{
        //    DS.db.DeleteMany<T>(a => true);
        //}

        //public List<T> Find(Expression<Func<T, bool>> filter)
        //{
        //    return DS.db.GetAll<T>(filter);
        //}

        //public List<T> Find(string field, object value, bool strict, int count)
        //{
        //    try
        //    {
        //        FilterDefinition<T> filt;

        //        if (value.GetType() == typeof(Guid))
        //        {
        //            filt = Builders<T>.Filter.Eq(field, Guid.Parse(value.ToString()));
        //        }
        //        else
        //        {
        //            if (strict)
        //            {
        //                filt = Builders<T>.Filter.Eq(field, value);
        //            }
        //            else
        //            {
        //                filt = Builders<T>.Filter.Regex(field, new BsonRegularExpression(value?.ToString(), "i"));
                      
        //            }
        //        } 
        //        return DS.db.HandlePartitioned<T>(null).FindSync<T>(filt).Current.ToList(); 
        //    }
        //    catch (Exception s)
        //    {
        //        DataHelpers.ShowMessage(s.Message + $"FIELD: {field} VALUE: {value}");
        //        return null;
        //    }
        //}

        //public List<T> Find(string field, object value, bool strict)
        //{
        //    try
        //    {
        //        FilterDefinition<T> filt;

        //        if (value.GetType() == typeof(Guid))
        //        {
        //            filt = Builders<T>.Filter.Eq(field, Guid.Parse(value.ToString()));
        //        }
        //        else
        //        {
        //            if (strict)
        //            {
        //                filt = Builders<T>.Filter.Eq(field, value);
        //            }
        //            else
        //            {
        //                filt = Builders<T>.Filter.Regex(field, new BsonRegularExpression(value?.ToString(), "i"));
        //            }
        //        }
                 
        //        return DS.db.HandlePartitioned<T>(null).FindSync<T>(filt).ToList(); 
        //    }
        //    catch (Exception s)
        //    {
        //         DataHelpers.ShowMessage(s.Message + $"FIELD: {field} VALUE: {value}");
        //        return null;
        //    }
        //}

        //public IEnumerable<IDocument> FindAll()
        //{
        //    return Find(a => true) as IEnumerable<IDocument>;
        //}

        //public async Task<IEnumerable<IDocument>> FindAllAsync()
        //{
        //    return await FindAsync(a => true) as IEnumerable<IDocument>;
        //}

        //public async Task<List<T>> FindAsync(Expression<Func<T, bool>> filter)
        //{
        //    return await DS.db.GetAllAsync<T>(filter);
        //}

        //public T GetById(Guid id)
        //{
        //    return DS.db.GetOne<T>(a => a.Id.Equals(id));
        //}

        //public T GetByIdNullable(Guid id)
        //{
        //    if (id.HasValue)
        //        return GetById(id.Value);
        //    return null;
        //}

        ////public T GetByIdNullable(Guid id)
        ////{
        ////    return GetById(id); 
        ////}

        //public async Task<long> GetCount()
        //{
        //    return await DS.db.CountAsync<T>(a => true);
        //}

        //public dynamic GetExpressionForSearch()
        //{
        //    return typeof(Expression<Func<T, bool>>);
        //}

        //public async Task<IEnumerable<dynamic>> GetRange(int count)
        //{
        //    var s = await DS.db.GetPaginatedAsyncA<T>(a => true, 0, count) as IEnumerable<dynamic>;
        //    return s;
        //}
    }
}