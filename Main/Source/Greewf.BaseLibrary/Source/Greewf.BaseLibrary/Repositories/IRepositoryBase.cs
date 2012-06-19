using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Greewf.BaseLibrary.Repositories
{
    public interface IRepositoryBase<T> where T : new()
    {
        IQueryable<T> All { get; }
        IQueryable<T> AllIncluding(params Expression<Func<T, object>>[] includeProperties);
        T Find(int id);
        T Find(int id, params Expression<Func<T, object>>[] includeProperties);
        void InsertOrUpdate(T product);
        void Delete(int id);
        void Save();
        void DetachProduct(T product);
    }
}
