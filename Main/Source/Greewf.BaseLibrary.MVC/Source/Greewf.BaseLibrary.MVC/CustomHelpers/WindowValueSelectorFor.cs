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

        #region WindowValueSelectorFor

        public static MvcHtmlString WindowValueSelectorFor<TModel, TValueProperty, TTitleProperty>(
           this HtmlHelper<TModel> helper,
           System.Linq.Expressions.Expression<Func<TModel, TValueProperty>> ValueExpression,
           System.Linq.Expressions.Expression<Func<TModel, TTitleProperty>> TitleExpression,
           string viewerUrlPrefix,
           string selectorUrl,
           string callBackWindowData_TitleFieldName,
           string callBackWindowData_IdFieldName)
        {
            return WindowValueSelectorFor(helper, ValueExpression, TitleExpression, viewerUrlPrefix, selectorUrl, callBackWindowData_TitleFieldName, callBackWindowData_IdFieldName, "...", "t-button window-selector", "نمایش", "t-button window-viewer", null, "حذف", "t-button window-selector");

        }


        public static MvcHtmlString WindowValueSelectorFor<TModel, TValueProperty, TTitleProperty>(
            this HtmlHelper<TModel> helper,
            System.Linq.Expressions.Expression<Func<TModel, TValueProperty>> valueExpression,
            System.Linq.Expressions.Expression<Func<TModel, TTitleProperty>> titleExpression,
            string viewerUrlPrefix,
            string selectorUrl,
            string callBackWindowData_TitleFieldName,
            string callBackWindowData_IdFieldName,
            string selectorInnerHtml,
            string selectorCssClass,
            string viewerInnerHtml,
            string viewerCssClass,
            string onClientChangeFunctionName = null,
            string removerInnerHtml = null,
            string removerCssClass = null
            )
        {
            string fullId = helper.GetFullPropertyName(valueExpression);
            string id = helper.GetPropertyName(valueExpression);
            string titleName = helper.GetPropertyName(titleExpression);
            string titleFullName = helper.GetFullPropertyName(titleExpression);
            string viewerName = fullId + "viewer";
            string removerName = fullId + "remover";
            string selectorName = fullId + "selector";
            string selectorCallBack = selectorName + "_CallBack";

            var url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            object value = ModelMetadata.FromLambdaExpression(valueExpression, helper.ViewData).Model;
            object titleValue = ModelMetadata.FromLambdaExpression(titleExpression, helper.ViewData).Model;

            //RULE : zero == null OR titleValue==null/empty means no item selected
            bool hasValue = !(value == null || titleValue == null || string.IsNullOrWhiteSpace(titleValue.ToString()));//becuase in some situations (like when the value is ZERO) titlevalue can help us to decide if it is null or not
            if (hasValue == false)
            {
                value = null;
                titleValue = null;
            }

            StringBuilder output = new StringBuilder();

            output.Append(helper.TextBox(titleName, titleValue, new { ReadOnly = true, Title = titleValue }).ToHtmlString());//helper.textbox is wise enough to correct the name regard of its container
            output.AppendFormat(
               "<a id='{0}' class='t-link {6}' justwindow='true' href='{1}' ajax='1' style='display:{2}'>{7}</a><a id='{10}' class='t-link {11}' href='#' style='display:{12}'>{13}</a><a id='{3}' ajax='1' href='{4}' windowcallback='{5}' class='t-link {8}' justwindow='true'>{9}</a>",
               viewerName,
               viewerUrlPrefix + value,
               hasValue ? "" : "none",
               selectorName,
               selectorUrl,
               selectorCallBack,
               viewerCssClass,
               viewerInnerHtml,
               selectorCssClass,
               selectorInnerHtml,
               removerName,
               removerCssClass,
               hasValue ? "" : "none",
               removerInnerHtml
            );

            //NOTE: 1- Hidden helper method generates "0" for null "value". so we should explicitly set it
            //      2- Hidden helper (unlike HiddenFor) automatically generates prefix for the id and name. So we should avoid it!    
            output.Append(helper.Hidden(id, value ?? "").ToHtmlString());

            output.AppendFormat(@"<script type='text/javascript'>function {0}(sender,data){{ {1}{2}{3}{4}{5}{6} }}</script>",
                selectorCallBack,
                string.Format("$('#{0}').attr('value', data.{1});", titleFullName, callBackWindowData_TitleFieldName),
                string.Format("$('#{0}').attr('title', data.{1});", titleFullName, callBackWindowData_TitleFieldName),
                string.Format("$('#{0}').attr('value', data.{1});", fullId, callBackWindowData_IdFieldName),
                string.Format("$('#{0}').attr('href', '{1}' + $('#{2}').attr('value')).css('display', 'inline-block');", viewerName, viewerUrlPrefix, fullId),
                string.Format("$('#{0}').css('display', '');", removerName),
                onClientChangeFunctionName == null ? null : string.Format("{0}(sender,data);", onClientChangeFunctionName)
            );

            //remove button
            output.AppendFormat(@"<script type='text/javascript'>$('#{0}').click(function(){{ {1}{2}{3}{4}{5} }})</script>",
                removerName,
                string.Format("$('#{0}').attr('value', '');", titleFullName),
                string.Format("$('#{0}').attr('title', '');", titleFullName),
                string.Format("$('#{0}').attr('value', '');", fullId),
                string.Format("$('#{0}').attr('href', '').css('display', 'none');", viewerName),
                string.Format("$('#{0}').css('display', 'none');", removerName)
            );

            return new MvcHtmlString(output.ToString());

        }

        #endregion

    }

}