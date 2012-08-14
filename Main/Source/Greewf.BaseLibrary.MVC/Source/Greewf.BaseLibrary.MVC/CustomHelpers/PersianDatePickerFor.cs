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


        public static MvcHtmlString PersianDatePickerFor<TModel, TProperty>(this HtmlHelper<TModel> helper, System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression, bool readOnly = false)
        {
            return PersianDatePickerFor(helper, expression, null, readOnly);
        }

        public static MvcHtmlString PersianDatePickerFor<TModel, TProperty>(this HtmlHelper<TModel> helper, System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression, IDictionary<string, object> htmlAttributes, bool readOnly = false)
        {
            string name = helper.ViewData.TemplateInfo.GetFullHtmlFieldName(ExpressionHelper.GetExpressionText(expression));
            var value = ModelMetadata.FromLambdaExpression(expression, helper.ViewData).Model as DateTime?;
            return PersianDatePicker(helper, name, value, htmlAttributes, readOnly);

        }


        public static MvcHtmlString PersianDatePicker(this HtmlHelper helper, string name, DateTime? value, IDictionary<string, object> htmlAttributes, bool readOnly = false, bool enableTextEntering = false)
        {

            StringBuilder output = new StringBuilder();

            string inputName = name;
            if (name == "")
                inputName= helper.ViewData.TemplateInfo.GetFullHtmlFieldName("");           
            string inputId = inputName.Replace(".", "");

            var attrs = htmlAttributes ?? new Dictionary<string, object>();
            attrs["class"] = "pcalendar";
            if (!enableTextEntering) attrs["readOnly"] = "readOnly";

            var input = helper.TextBox(name, Greewf.BaseLibrary.Global.DisplayDate(value), attrs);
            var span = new TagBuilder("span");

            if (!readOnly)
            {
                span.MergeAttribute("id", "dpb" + inputId);
                span.MergeAttribute("class", "t-icon t-icon-calendar");
                span.MergeAttribute("style", "vertical-align:text-bottom;cursor:pointer");
                span.MergeAttribute("title", "باز کردن تقویم");
                span.InnerHtml = "باز کردن تقویم";

                var script = new TagBuilder("script");
                script.MergeAttribute("type", "text/javascript");
                script.InnerHtml = string.Format("$(document).ready(function(){{ $('#dpb{0}').click(function () {{ $('#{0}').focus(); }}); }});", inputId);
                output.Append(script.ToString());

            }
            else//readonly
                attrs["disabled"]= "disabled";

            output.Append(input.ToHtmlString());
            output.Append(span.ToString());

            return new MvcHtmlString(output.ToString());
        }



    }

}