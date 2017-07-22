using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Runtime.CompilerServices;

namespace Greewf.BaseLibrary.MVC.TelerikExtentions
{
    public static class TelerikHelper
    {
        public static bool IsGridModelRelated()
        {
            if (HttpContext.Current.Request.HttpMethod == "POST" && 
                (HttpContext.Current.Request.Headers["X-Requested-With"] == "XMLHttpRequest" || HttpContext.Current.Request["exportToExcel"] != null))
                return true;
            return false;
        }
    }
}
