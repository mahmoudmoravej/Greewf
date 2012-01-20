using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace Greewf.BaseLibrary.Repositories
{
    public abstract class ContextManagerBase
    {

    }

    public abstract class ContextManager<T> : ContextManagerBase
        where T : DbContext, new()
    {
        internal T Context { get; private set; }
        public ContextManager()
        {
            Context = new T();
        }
    }
}
