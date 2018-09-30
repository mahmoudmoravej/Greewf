using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Greewf.BaseLibrary.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Entity</typeparam>
    /// <typeparam name="Y">Entity's Key</typeparam>
    public interface IRepositoryBase2<T, Y> where T : new()
    {
        IQueryable<T> All { get; }
        IQueryable<T> AllIncluding(params Expression<Func<T, object>>[] includeProperties);
        T Find(Y id);
        T Find(Y id, params Expression<Func<T, object>>[] includeProperties);
        bool InsertOrUpdate(T entity, object etc = null);
        bool InsertRange(IEnumerable<T> entities, Dictionary<T, object> etcs = null);
        bool Delete(Y id, object etc = null);
        bool Delete(T entity, object etc = null);
        bool Delete(Y[] ids, Dictionary<Y, object> etcs = null);
        bool DeleteRange(IEnumerable<T> entities, Dictionary<T, object> etcs = null);
        bool DeleteRange(Y[] ids, Dictionary<Y, object> etcs = null);
        void Save();
        void Detach(T entity, object etc = null);
        bool Validate(T entity, object etc = null);
        bool ValidateDeleting(T entity, object etc = null);
    }
}
