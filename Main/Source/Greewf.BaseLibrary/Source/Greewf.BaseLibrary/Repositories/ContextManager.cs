using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace Greewf.BaseLibrary.Repositories
{
    public abstract class ContextManagerBase
    {
        public DbContext ContextBase { get; protected set; } //we need this in ChangesTracker module.
    }

    public abstract class ContextManager<T> : ContextManagerBase
        where T : DbContext, new()
    {
        public IValidationDictionary ValidationDictionary { get; private set; }

        public T Context { get; private set; }
        public ContextManager(IValidationDictionary validationDictionary)
        {
            Context = new T();
            ContextBase = Context;
            ValidationDictionary = validationDictionary ?? new DefaultValidationDictionary();
        }

        public virtual int SaveChanges()
        {
            if (ValidationDictionary != null && !ValidationDictionary.IsValid)
                throw new Exception("Greewf: the ValidationDictionary is not valid!");
            return Context.SaveChanges();
        }
    }
}
