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


        public static MvcHtmlString PersianDatePicker(this HtmlHelper helper, string name, DateTime? value, IDictionary<string, object> htmlAttributes, bool readOnly = false)
        {

            StringBuilder output = new StringBuilder();
            string inputName = name;
            string inputId = inputName.Replace(".", "");

            var input = new TagBuilder("input");

            if (htmlAttributes != null)
                foreach (var item in htmlAttributes)
                    input.MergeAttribute(item.Key, item.Value as string);

            input.MergeAttribute("type", "text");
            input.MergeAttribute("class", "pcalendar");
            input.MergeAttribute("readonly", "readonly");
            input.MergeAttribute("value", Greewf.BaseLibrary.Global.DisplayDate(value));
            input.MergeAttribute("id", inputId);
            input.MergeAttribute("name", inputName);



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
                input.MergeAttribute("disabled", "disabled");

            output.Append(input.ToString(TagRenderMode.SelfClosing));
            output.Append(span.ToString());

            return new MvcHtmlString(output.ToString());
        }



    }

}