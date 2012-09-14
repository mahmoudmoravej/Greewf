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
    public interface IRepositoryBase<T,Y> where T : new()
    {
        IQueryable<T> All { get; }
        IQueryable<T> AllIncluding(params Expression<Func<T, object>>[] includeProperties);
        T Find(Y id);
        T Find(Y id, params Expression<Func<T, object>>[] includeProperties);
        void InsertOrUpdate(T entity);
        void Delete(Y id);
        void Save();
        void Detach(T entity);
    }
}
