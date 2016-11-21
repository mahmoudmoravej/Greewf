using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using effts;
using System.Data.Entity.Infrastructure.Interception;
using System.Transactions;

namespace Greewf.BaseLibrary.Repositories
{
    public abstract class ContextManagerBase
    {
        public DbContext ContextBase { get; protected set; } //we need this in ChangesTracker module.     
                                                             // public event Action OnChangesCommitted;

        //protected internal void InvokeOnChangesCommittedEvent()
        //{
        //    OnChangesCommitted?.Invoke();
        //}
    }

    public abstract class ContextManager<T> : ContextManagerBase
        where T : DbContext, ITransactionScopeAwareness, new()
    {
        private static bool _isFullTextSearchInitiated = false;

        public IValidationDictionary ValidationDictionary { get; private set; }

        public T Context { get; private set; }
        public ContextManager(IValidationDictionary validationDictionary)
        {
            Context = new T();
            ContextBase = Context;
            ValidationDictionary = validationDictionary ?? new DefaultValidationDictionary();
            if (!_isFullTextSearchInitiated)
            {
                _isFullTextSearchInitiated = true;
                DbInterception.Add(new FtsInterceptor());
            }
        }

        public virtual bool SaveChanges()
        {
            if (ValidationDictionary != null && !ValidationDictionary.IsValid)
                return false;

            bool result = true;

            if (Context.TransactionScope != null)//to avoid creating new scope (it comes when savechanges causes to call savechanges again)
            {
                Context.SaveChanges();
                if (ValidationDictionary?.IsValid == false)
                    result = false;

                return result;
            }

            using (var scope = new TransactionScope())
            {
                Context.TransactionScope = scope;
                Context.SaveChanges();//my be some calls on onChangesSaved event that needs to be in transaction

                if (ValidationDictionary?.IsValid == false)
                    result = false;

                if (result)
                    scope.Complete();
                else
                    scope.Dispose();
            }

            Context.TransactionScope = null;            

            if (result)
            {
                //  InvokeOnChangesCommittedEvent();              
                Context.OnChangesCommittedEventInvoker();
            }

            return result;
        }

    }
}
