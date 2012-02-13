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
    public static  partial class CustomHelper
    {    

        #region Validation Summary

        public static MvcHtmlString ValidationSummaryInWindow<TModel>(this HtmlHelper<TModel> helper, bool excludePropertyErrors, string title = null, string message = null, int? width = null, int? height = null)
        {
            //NOTE : have some problems in javascript...and more important :: have problem with unobtrusive javascript!
            return ValidationSummaryInWindowBase(helper, null, title, excludePropertyErrors, helper.ValidationSummary(excludePropertyErrors, message), width, height);
        }

        public static MvcHtmlString ValidationSummaryInWindow<TModel>(this HtmlHelper<TModel> helper, string name, bool excludePropertyErrors, string title = null, string message = null, int? width = null, int? height = null)
        {
            return ValidationSummaryInWindowBase(helper, name, title, excludePropertyErrors, helper.ValidationSummary(excludePropertyErrors, message), width, height);
        }


        //private static MvcHtmlString ValidationSummaryInWindowBase<TModel>(HtmlHelper<TModel> helper, string name, string title, bool excludePropertyErrors, MvcHtmlString pureValidationSummary, int? width = null, int? height = null)
        //{
        //    name = string.IsNullOrWhiteSpace(name) ? "ErrorWindow" : name;
        //    StringBuilder output = new StringBuilder();
        //    StringBuilder outputScript = new StringBuilder();

        //    output.Append(
        //         helper.Telerik()
        //         .Window()
        //         .Name(name)
        //         .Title(title ?? "خطاهای ثبت اطلاعات")
        //         .Draggable(true)
        //         .Buttons(b =>
        //         {
        //             b.Close();
        //         })
        //         .Modal(false)
        //         .Resizable(r =>
        //         {
        //             r.Enabled(true);
        //         })
        //         .Visible(false)
        //         .Scrollable(true)
        //         .Resizable(r => r.Enabled(true))
        //         .Width(width ?? 250)
        //         .Height(height ?? 100)
        //         .Content(pureValidationSummary == null ? "" : pureValidationSummary.ToHtmlString()).ToHtmlString()
        //    );

        //    outputScript.AppendFormat("<script type='text/javascript'> $(document).ready(function () {{ {0} }});</script>",
        //        string.Format("$('#{0}').closest('form').submit(function () {{if ({0} && Sys.Mvc.FormContext && Sys.Mvc.FormContext.getValidationForForm(this).validate('submit').length) $('#{1}').data('tWindow').center().open();}});", excludePropertyErrors ? "false" : "true", name)
        //        );
        //    output.Append(outputScript);
        //    helper.Telerik().ScriptRegistrar().OnDocumentReady(string.Format(" if ($('#validationSummary','#{0}').hasClass('validation-summary-errors')) {{ $('#{0}').data('tWindow').center().open(); }}", name));
        //    return new MvcHtmlString(output.ToString());
        //}

        private static MvcHtmlString ValidationSummaryInWindowBase<TModel>(HtmlHelper<TModel> helper, string name, string title, bool excludePropertyErrors, MvcHtmlString pureValidationSummary, int? width = null, int? height = null)
        {
            name = string.IsNullOrWhiteSpace(name) ? "ErrorWindow" : name;
            StringBuilder output = new StringBuilder();
            StringBuilder outputScript = new StringBuilder();

            output.Append(string.Format("<div id='{0}' style='display:none;'>{1}</div>", name, (pureValidationSummary == null ? "" : pureValidationSummary.ToHtmlString())));

            outputScript.AppendFormat("<script type='text/javascript'> $(document).ready(function () {{ {0} }});</script>",
                string.Format("$('#{1}').closest('form').submit(function () {{if ({0} && Sys.Mvc.FormContext && Sys.Mvc.FormContext.getValidationForForm(this).validate('submit').length) layoutHelper.core.showErrorMessage($('#{1}').html(),'بروز خطا'); }});", excludePropertyErrors ? "false" : "true", name)
                );
            output.Append(outputScript);
            helper.Telerik().ScriptRegistrar().OnDocumentReady(string.Format(" if ($('#validationSummary','#{0}').hasClass('validation-summary-errors')) {{ layoutHelper.core.showErrorMessage($('#{0}').html(),'بروز خطا');  }}", name));
            return new MvcHtmlString(output.ToString());
        }


        #endregion

    }

}