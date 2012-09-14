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
        public T Context { get; private set; }
        public ContextManager()
        {
            Context = new T();
            ContextBase = Context;
        }

        public virtual int SaveChanges()
        {
            return Context.SaveChanges();
        }
    }
}
