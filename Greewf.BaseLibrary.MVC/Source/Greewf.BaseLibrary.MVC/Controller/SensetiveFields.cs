using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Greewf.BaseLibrary.MVC.Security;
using System.Web.Routing;
using System.Text;
using System.Data.Entity;
using Greewf.BaseLibrary.Repositories;
using Greewf.BaseLibrary.MVC.Logging;
using Greewf.BaseLibrary.MVC.Ajax;
using System.Linq.Expressions;

namespace Greewf.BaseLibrary.MVC
{

    public class SensetiveFields<T> : List<Expression<Func<T, object>>>
    {
        public new SensetiveFields<T> Add(Expression<Func<T, object>> field)
        {
            base.Add(field);
            return this;
        }

        public string[] ToStringArray()
        {
            return this.Select(o =>
            {
                if (o.Body is UnaryExpression)
                    return ((MemberExpression)((UnaryExpression)o.Body).Operand).Member.Name;
                else
                    return ExpressionHelper.GetExpressionText(o);
            }).ToArray();
        }

    }


}