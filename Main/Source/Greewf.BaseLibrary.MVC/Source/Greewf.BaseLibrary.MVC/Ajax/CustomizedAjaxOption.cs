using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc.Ajax;
using System.Web.Mvc;

namespace Greewf.BaseLibrary.MVC.Ajax
{
    public class CustomizedAjaxOption : AjaxOptions
    {
        public CustomizedAjaxOption(string updatePanelId,string progressCssClass)
        {
            if (HtmlHelper.UnobtrusiveJavaScriptEnabled)
            {
                OnSuccess = string.Format("$('#{0}').html(arguments[0]);", updatePanelId);
                OnBegin = string.Format("$('#{0}').html(\"<div class='{1}'></div>\");", updatePanelId, progressCssClass);
            }
            else
            {
                OnSuccess = string.Format("function(ajaxContext) {{ $('#{0}').html(ajaxContext.get_data()); }}", updatePanelId);
                OnBegin = string.Format("function() {{ $('#{0}').html(\"<div class='{1}'></div>\"); }}", updatePanelId, progressCssClass);
            }
        }
    }
}
