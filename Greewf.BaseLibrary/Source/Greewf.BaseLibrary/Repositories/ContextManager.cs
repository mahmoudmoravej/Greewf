using effts;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
using System.Threading.Tasks;
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
        public ContextManager(IValidationDictionary validationDictionary, Func<T> contextInstantiator = null)
        {
            Context = contextInstantiator == null ? new T() : contextInstantiator();
            ContextBase = Context;
            ValidationDictionary = validationDictionary ?? new DefaultValidationDictionary();
            if (!_isFullTextSearchInitiated)
            {
                _isFullTextSearchInitiated = true;
                DbInterception.Add(new FtsInterceptor());
            }
        }

        //NOTE!!!!!!!!!!!!!! تغییرات را در هر دو متد یکسان کنید
        //با توجه به توضیح داده شده در اینجا، دو ورژن نوشتمی : https://stackoverflow.com/a/14876375/790811
        public virtual bool SaveChanges()
        {
            if (ValidationDictionary != null && !ValidationDictionary.IsValid)
                return false;

            bool result = true;

            //to avoid creating new scope (it comes when savechanges causes to call savechanges again). 
            //شرط دوم برای این است که ممکن است کانتکست از ابتدا با یک کانکشنی ایجاد شده باشد که خود در درون ترنزکشن است
            //در این حالت ایجاد ترنزکشن اسکوپ باعث می شود خطای روبرو را بگیریم : "Cannot enlist in the transaction because a local transaction is in progress on the connection"
            //همچنین توجه کنید نیازی به فراخوانی های ایونت ها نیست. چراکه در واقع مانند یک کانتکست معمولی با ان برخورد می شود
            //و پس از فراخوانی ذخیره، خود کانتکست چون می داند در اسکوپ نیست، ایونت کامیت را فراخوانی می کند
            //و ازآنجا که این ایونت مختص زمانیکه فراخوانی می شود که از کامیت خود مطمن باشد صحیح هست. حال مهم نیست که 
            //تراکنشی بیرون تر آنرا دارد مدیریت می کند یا خیر
            if (Context.IsInActiveTransactionScope || Context.Database.CurrentTransaction != null)
            {
                Context.SaveChanges();
                if (ValidationDictionary?.IsValid == false)
                    result = false;

                return result;
            }

            try
            {
                Context.OnBeforeTransactionScopeStart();

                Context.Database.Connection.Close();
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled/*due to https://stackoverflow.com/q/13543254/790811 to be able to support async cases*/ ))
                {
                    Context.IsInActiveTransactionScope = true;
                     Context.SaveChanges();//my be some calls on onChangesSaved event that needs to be in transaction

                    if (ValidationDictionary?.IsValid == false)
                        result = false;

                    if (result)
                        scope.Complete();
                    else
                        scope.Dispose();
                }
            }
            finally
            {
                Context.IsInActiveTransactionScope = false;
            }



            if (result)
                Context.OnTransactionScopeCommitted();
            else
                Context.OnTransactionScopeRollbacked();

            return result;
        }

        //NOTE!!!!!!!!!!!!!! تغییرات را در هر دو متد یکسان کنید
        //با توجه به توضیح داده شده در اینجا، دو ورژن نوشتمی : https://stackoverflow.com/a/14876375/790811
        public virtual async Task<bool> SaveChangesAsync()
        {
            if (ValidationDictionary != null && !ValidationDictionary.IsValid)
                return false;

            bool result = true;

            //to avoid creating new scope (it comes when savechanges causes to call savechanges again). 
            //شرط دوم برای این است که ممکن است کانتکست از ابتدا با یک کانکشنی ایجاد شده باشد که خود در درون ترنزکشن است
            //در این حالت ایجاد ترنزکشن اسکوپ باعث می شود خطای روبرو را بگیریم : "Cannot enlist in the transaction because a local transaction is in progress on the connection"
            //همچنین توجه کنید نیازی به فراخوانی های ایونت ها نیست. چراکه در واقع مانند یک کانتکست معمولی با ان برخورد می شود
            //و پس از فراخوانی ذخیره، خود کانتکست چون می داند در اسکوپ نیست، ایونت کامیت را فراخوانی می کند
            //و ازآنجا که این ایونت مختص زمانیکه فراخوانی می شود که از کامیت خود مطمن باشد صحیح هست. حال مهم نیست که 
            //تراکنشی بیرون تر آنرا دارد مدیریت می کند یا خیر
            if (Context.IsInActiveTransactionScope || Context.Database.CurrentTransaction != null)
            {
                await Context.SaveChangesAsync();
                if (ValidationDictionary?.IsValid == false)
                    result = false;

                return result;
            }

            try
            {
                Context.OnBeforeTransactionScopeStart();

                Context.Database.Connection.Close();
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled/*due to https://stackoverflow.com/q/13543254/790811 to be able to support async cases*/ ))
                {
                    Context.IsInActiveTransactionScope = true;
                    await Context.SaveChangesAsync();//my be some calls on onChangesSaved event that needs to be in transaction

                    if (ValidationDictionary?.IsValid == false)
                        result = false;

                    if (result)
                        scope.Complete();
                    else
                        scope.Dispose();
                }
            }
            finally
            {
                Context.IsInActiveTransactionScope = false;
            }



            if (result)
                Context.OnTransactionScopeCommitted();
            else
                Context.OnTransactionScopeRollbacked();

            return result;
        }

  

    }
}
