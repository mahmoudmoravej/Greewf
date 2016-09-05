//Source : http://stackoverflow.com/a/26762756/790811

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greewf.BaseLibrary.Linq
{
    public class HintScope : IDisposable
    {
        public IQueryHintContext Context { get; private set; }
        public void Dispose()
        {
            Context.ApplyHint = false;
            Context.QueryHint = null;
        }

        public HintScope(IQueryHintContext context, string hint)
        {
            if (!HintInterceptor.IsRegistered) HintInterceptor.Register();

            Context = context;
            Context.ApplyHint = true;
            Context.QueryHint = hint;
        }
    }
}
