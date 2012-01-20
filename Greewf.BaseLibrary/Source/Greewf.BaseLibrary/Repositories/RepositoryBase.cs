using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Linq.Expressions;
using Greewf.BaseLibrary.Repositories;

namespace Greewf.BaseLibrary.Repositories
{
    /// <summary>
    /// The main EntityFrmaework DbContext
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RepositoryBase<T>
        where T : DbContext, new()
    {
        protected T context = null;

        protected RepositoryBase(ContextManager<T> contextManager)
        {
            if (contextManager == null)
                context = new T();// throw new Exception("ContextManager cannot be empty for Repository creation");
            else
                context = contextManager.Context;
        }

        protected IQueryable<X> AllIncluding<X>(DbSet<X> dbset, params Expression<Func<X, object>>[] includeProperties) where X : class ,new()
        {
            IQueryable<X> query = dbset;
            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public void Detach<E>(E entity) where E : class
        {
            context.Entry<E>(entity).State = System.Data.EntityState.Detached;
        }

    }

}
