using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Mvc.Html;
using Telerik.Web.Mvc.UI;
using AutoMapper;

namespace Greewf.BaseLibrary.MVC.CustomHelpers
{
    public static partial class CustomHelper
    {    

        #region PartialFor

        public static MvcHtmlString PartialFor<TModel, TProperty>(this HtmlHelper<TModel> helper, System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression, string partialViewName)
        {
            return helper.PartialFor(expression, null, partialViewName);
        }

        public static MvcHtmlString PartialFor<TModel, TProperty>(this HtmlHelper<TModel> helper, System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression, Type TMap, string partialViewName)
        {
            string name = ExpressionHelper.GetExpressionText(expression);
            var model = ModelMetadata.FromLambdaExpression(expression, helper.ViewData).Model;

            if (TMap != null)
            {
                var mapped = AutoMapper.Mapper.Map(model, typeof(TProperty), TMap);
                return CreateParialHtml(helper, mapped, name, partialViewName);
            }
            else
                return CreateParialHtml(helper, model, name, partialViewName);

        }

        private static MvcHtmlString CreateParialHtml(HtmlHelper helper, object model, string prefixName, string partialViewName)
        {
            var viewData = new ViewDataDictionary(helper.ViewData)
            {
                TemplateInfo = new System.Web.Mvc.TemplateInfo
                {
                    HtmlFieldPrefix = prefixName
                }
            };

            return helper.Partial(partialViewName, model, viewData);

        }

        #endregion
    }

}