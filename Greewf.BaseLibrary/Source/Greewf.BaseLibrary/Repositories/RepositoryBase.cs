using Greewf.BaseLibrary.Linq;
using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Greewf.BaseLibrary.Repositories
{
    /// <summary>
    /// The main EntityFrmaework DbContext
    /// </summary>
    /// <typeparam name="T">EF Context</typeparam>
    /// <typeparam name="Y">UnitOfRepository</typeparam>
    public class RepositoryBase<T, Y>
        where T : DbContext, ISavingTracker, ITransactionScopeAwareness, IQueryHintContext, new()        
    {
        protected T context = null;
        protected ContextManager<T> ContextManager { get; private set; }
        protected IValidationDictionary ValidationDictionary { get; private set; }//we cannot return it directly from ContextManager becuase sometimes it may be null


        protected RepositoryBase(ContextManager<T> contextManager, Func<Y> unitOfRepositoryInstantiator)
        {
            ContextManager = contextManager;
            if (contextManager == null)
            {
                context = new T();// throw new Exception("ContextManager cannot be empty for Repository creation");
                ValidationDictionary = new DefaultValidationDictionary();//when contextmanager is null.                
            }
            else
            {
                context = contextManager.Context;
                ValidationDictionary = ContextManager.ValidationDictionary;
            }

            //handling events
            context.OnChangesSaving += OnChangesSaving;
            context.OnChangesSaved += OnChangesSaved;
            context.OnChangesCommitted += OnChangesCommitted;
            context.OnBeforeTransactionStart += OnBeforeTransactionStart;

            //if (contextManager == null)//because committing transaction is handled with context manager. when there is no any context manager, so we don't have any outer transaction(transactionscope indeed). so when the changes saved, it means commission too.
            //{
            //    context.OnChangesSaved += (o) =>
            //    {
            //        context.OnChangesCommittedEventInvoker();
            //    };
            //}

            _UoRInstantiator = unitOfRepositoryInstantiator;
        }

        private Func<Y> _UoRInstantiator;
        private Y _UoR;
        protected Y UoR
        {
            get
            {
                if (_UoR == null)
                {
                    _UoR = _UoRInstantiator();
                }

                return _UoR;
            }
        }


        protected virtual void OnChangesSaving(DbContext context)
        {
        }

        protected virtual void OnChangesSaved(DbContext context)
        {
        }

        protected virtual void OnChangesCommitted()
        {

        }

        protected virtual void OnBeforeTransactionStart()
        {

        }

        protected IQueryable<X> AllIncluding<X>(DbSet<X> dbset, params Expression<Func<X, object>>[] includeProperties) where X : class, new()
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
            context.Entry<E>(entity).State = EntityState.Detached;
        }

        public string[] Errors
        {
            get
            {
                return ValidationDictionary.Errors;
            }
        }

    }

    public class RepositoryBase<T, Y, M> : RepositoryBase<T, Y>
        where T : DbContext, ISavingTracker, ITransactionScopeAwareness, IQueryHintContext, new()        
        where M : class, new()
    {
        //NOTE! this class is just for generic ValidationDictionart<M>. please put your extensibility codes in base class.

        public RepositoryBase(ContextManager<T> contextManager, Func<Y> unitOfRepositoryInstantiator)
            : base(contextManager, unitOfRepositoryInstantiator)
        {
        }

        protected new IValidationDictionary<M> ValidationDictionary
        {
            get
            {
                return base.ValidationDictionary;
            }
        }

    }
}
