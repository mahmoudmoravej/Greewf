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


        public static IHtmlString ShowDiffsAsHtml<TModel>(this HtmlHelper<TModel> helper, string oldText, string newText)
        {
            return helper.Raw(Global.ShowDiffsAsHtml(oldText, newText));
        }

        public static IHtmlString ShowDiffsAsHtml<TModel, TProperty>(this HtmlHelper helper, string oldText, string newText)
        {
            return helper.Raw(Global.ShowDiffsAsHtml(oldText, newText));
        }


    }


}