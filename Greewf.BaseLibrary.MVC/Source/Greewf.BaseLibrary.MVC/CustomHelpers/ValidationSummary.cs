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

        /// <summary>
        /// توجه!!!!!!!! : این کنترل وابسته به اسکریپت های مایکروسافت است
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="helper"></param>
        /// <param name="excludePropertyErrors"></param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static MvcHtmlString ValidationSummaryInWindow<TModel>(this HtmlHelper<TModel> helper, bool excludePropertyErrors, string title = null, string message = null, int? width = null, int? height = null)
        {
            //NOTE : have some problems in javascript...and more important :: have problem with unobtrusive javascript!
            return ValidationSummaryInWindowBase(helper, null, title, excludePropertyErrors, helper.ValidationSummary(excludePropertyErrors, message), width, height);
        }

        public static MvcHtmlString ValidationSummaryInWindow<TModel>(this HtmlHelper<TModel> helper, string name, bool excludePropertyErrors, string title = null, string message = null, int? width = null, int? height = null)
        {
            return ValidationSummaryInWindowBase(helper, name, title, excludePropertyErrors, helper.ValidationSummary(excludePropertyErrors, message), width, height);
        }



        private static MvcHtmlString ValidationSummaryInWindowBase<TModel>(HtmlHelper<TModel> helper, string name, string title, bool excludePropertyErrors, MvcHtmlString pureValidationSummary, int? width = null, int? height = null)
        {
            name = string.IsNullOrWhiteSpace(name) ? "ErrorWindow" : name;
            StringBuilder output = new StringBuilder();
            StringBuilder outputScript = new StringBuilder();

            output.Append(string.Format("<div id='{0}' style='display:none;'>{1}</div>", name, (pureValidationSummary == null ? "" : pureValidationSummary.ToHtmlString())));

            outputScript.AppendFormat("<script type='text/javascript'> $(document).ready(function () {{ {0} }});</script>",
                string.Format(@"$('#{1}').closest('form').submit(function () {{
                        if (!{0}) return;
                        if (window.Sys && Sys.Mvc.FormContext && Sys.Mvc.FormContext.getValidationForForm(this).validate('submit').length) 
                            layoutHelper.core.showErrorMessage($('#{1}').html(),'بروز خطا'); 
                        else if ($(this).valid && $(this).valid()==false)
                            layoutHelper.core.showErrorMessage($('#{1}').html(),'بروز خطا'); 
                        }}); ", excludePropertyErrors ? "false" : "true", name)
                );
            output.Append(outputScript);
            helper.Telerik().ScriptRegistrar().OnDocumentReady(string.Format(" if ($('#validationSummary','#{0}').hasClass('validation-summary-errors')) {{ layoutHelper.core.showErrorMessage($('#{0}').html(),'بروز خطا');  }}", name));
            return new MvcHtmlString(output.ToString());
        }


        #endregion

    }

}