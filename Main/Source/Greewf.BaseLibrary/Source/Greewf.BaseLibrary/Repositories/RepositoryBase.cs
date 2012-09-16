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
    /// <typeparam name="Y">UnitOfRepository</typeparam>
    public class RepositoryBase<T,Y>
        where T : DbContext, new()
        where Y : class , new()
    {
        protected T context = null;
        protected ContextManager<T> ContextManager { get; private set; }
        protected IValidationDictionary ValidationDictionary { get; private set; }
        protected Y UoR { get; private set; }

        protected RepositoryBase(ContextManager<T> contextManager, IValidationDictionary validationDictionary, Y unitOfRepository)
        {
            ContextManager = contextManager;
            if (contextManager == null)
                context = new T();// throw new Exception("ContextManager cannot be empty for Repository creation");
            else
                context = contextManager.Context;
            ValidationDictionary = validationDictionary ?? new DefaultValidationDictionary();
            UoR = unitOfRepository;

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

        public string[] Errors
        {
            get
            {
                return ValidationDictionary.Errors;
            }
        }

    }

}
