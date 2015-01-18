using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Greewf.BaseLibrary
{
    public static class WarningExtentions
    {
        public static string GetName<m, p>(Expression<Func<m, p>> exp)
        {
            MemberExpression body = exp.Body as MemberExpression;

            if (body == null)
            {
                UnaryExpression ubody = (UnaryExpression)exp.Body;
                body = ubody.Operand as MemberExpression;
            }

            return body.Member.Name;
        }

        public static void AddWarning<M, P>(this IWarningDictionary<M> v, Expression<Func<M, P>> exp, string errorMessage)
        {
            (v as IWarningDictionary).AddWarning(GetName(exp), errorMessage);
        }
    }
}
