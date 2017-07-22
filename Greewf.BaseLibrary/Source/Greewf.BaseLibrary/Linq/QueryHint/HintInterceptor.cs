//Source : http://stackoverflow.com/a/26762756/790811

using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greewf.BaseLibrary.Linq
{
    public class HintInterceptor : DbCommandInterceptor
    {
        internal static bool IsRegistered = false;
        public override void ReaderExecuting(System.Data.Common.DbCommand command, DbCommandInterceptionContext<System.Data.Common.DbDataReader> interceptionContext)
        {
            if (interceptionContext.DbContexts.Any(db => db is IQueryHintContext))
            {
                var ctx = interceptionContext.DbContexts.First(db => db is IQueryHintContext) as IQueryHintContext;
                var addingString = $" option ({ctx.QueryHint})";
                if (ctx.ApplyHint && command.CommandText != null && !command.CommandText.Contains(addingString))
                {
                    command.CommandText += addingString;
                }
            }
            base.ReaderExecuting(command, interceptionContext);
        }

        internal static void Register()
        {
            DbInterception.Add(new HintInterceptor());
        }
    }
}
