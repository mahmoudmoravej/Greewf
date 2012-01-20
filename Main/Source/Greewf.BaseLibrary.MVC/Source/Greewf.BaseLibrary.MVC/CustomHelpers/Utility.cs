using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Mvc.Html;
using Telerik.Web.Mvc.UI;
using AutoMapper;
using System.Web.Routing;

namespace Greewf.BaseLibrary.MVC.CustomHelpers
{
    public static partial class CustomHelper
    {
        public const string SearchCriteriaPrefix = "SearchCriteria";

        #region Utilities

        public static string GetUrl(string contentPath)
        {
            return (new UrlHelper(HttpContext.Current.Request.RequestContext)).Content(contentPath);
        }

        public static string GetPropertyName<TModel, TProperty>(this HtmlHelper<TModel> helper, System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression, bool replaceDots = true)
        {
            return GetPropertyName(expression, replaceDots);
        }

        public static string GetPropertyName<TModel, TProperty>(System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression, bool replaceDots = true)
        {
            if (replaceDots)
                return ExpressionHelper.GetExpressionText(expression).Replace(".", "_");
            else
                return ExpressionHelper.GetExpressionText(expression);
        }

        public static string GetFullPropertyName<TModel, TProperty>(this HtmlHelper<TModel> helper, System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression, bool replaceDots = true)
        {
            string result = helper.ViewData.TemplateInfo.GetFullHtmlFieldName(GetPropertyName(expression, replaceDots));
            if (replaceDots) result = result.Replace(".", "_");
            return result;
        }

        public static string GetFullPropertyName<TModel, TProperty>(this HtmlHelper helper, System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression, bool replaceDots = true)
        {
            string result = helper.ViewData.TemplateInfo.GetFullHtmlFieldName(GetPropertyName(expression, replaceDots));
            if (replaceDots) result = result.Replace(".", "_");
            return result;
        }

        public static string GetPropertyDisplayText<TModel, TProperty>(this HtmlHelper<TModel> helper, System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            return metadata.DisplayName;
        }

        public static RouteValueDictionary AppendSearchCriteria(this HtmlHelper helper, bool? isInSearchForm, object currentValues = null)
        {
            var lst = new RouteValueDictionary(currentValues);
            if (isInSearchForm == true)
            {
                var form = HttpContext.Current.Request.Form;
                foreach (var item in form.AllKeys)
                {
                    lst.Add(CustomHelper.SearchCriteriaPrefix + "." + item, form[item]);
                }
            }
            return lst;
        }

        public static RouteValueDictionary AppendSearchCriteria<T>(this HtmlHelper helper, T criteria, bool? isInSearchForm, object currentValues = null)
        {
            var values = AppendSearchCriteria(helper, isInSearchForm, currentValues);

            var lst = new RouteValueDictionary(values);
            if (criteria != null)
                foreach (var item in typeof(T).GetProperties(System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Instance))
                {
                    var valueObj = item.GetValue(criteria, null);
                    if (valueObj != null)
                    {
                        var value = valueObj.ToString();
                        lst.Add(CustomHelper.SearchCriteriaPrefix + "." + item.Name, value);
                    }
                }

            return lst;
        }

        public static MvcForm BeginForm(this AjaxHelper helper,string actionName , object newRouteValues, string[] removingQuerystringKeys, System.Web.Mvc.Ajax.AjaxOptions ajaxOptions)
        {
            var values = new RouteValueDictionary(newRouteValues);
            foreach (string key in HttpContext.Current.Request.QueryString.AllKeys)
            {
                if (!removingQuerystringKeys.Contains(key) && !values.ContainsKey(key))
                    values.Add(key, HttpContext.Current.Request.QueryString[key]);
            }

            return System.Web.Mvc.Ajax.AjaxExtensions.BeginForm(helper, actionName, values, ajaxOptions);
        }

        #endregion
    }

    public class SpecialSelectListItem : SelectListItem
    {
        public int? ParentId { get; set; }
        public int? Order { get; set; }
    }

}