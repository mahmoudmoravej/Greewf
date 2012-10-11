using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Greewf.BaseLibrary.MVC.Security;
using System.Web.Routing;
using System.Text;
using System.Data.Entity;
using Greewf.BaseLibrary.Repositories;
using Greewf.BaseLibrary.MVC.Logging;
using Greewf.BaseLibrary.MVC.Ajax;
using System.Linq.Expressions;

namespace Greewf.BaseLibrary.MVC
{    
    public static class RedirectToRouteResultExtensions
    {
        public static RedirectToRouteResultEx AddFragment(this RedirectToRouteResult result, string fragment)
        {
            return new RedirectToRouteResultEx(result.RouteName, result.RouteValues)
            {
                Fragment = fragment
            };
        }

        public static string ToString(this RedirectToRouteResult result, ControllerContext context)
        {
            var helper = new UrlHelper(context.RequestContext);
            return helper.RouteUrl(result.RouteName, result.RouteValues);
        }

        public static bool IsSavedSuccessfullyRedirect(this RedirectToRouteResult result)
        {
            if (result.RouteValues.ContainsValue(CustomizedControllerBase.SavedSuccessfullyActionName))
                return true;
            return false;

        }

        public static bool IsSavedSuccessfullyRedirect(this RedirectToRouteResultEx result)
        {
            if (result.Fragment.Contains(CustomizedControllerBase.SavedSuccessfullyFramgment))
                return true;
            else if (result.RouteValues.ContainsValue(CustomizedControllerBase.SavedSuccessfullyActionName))
                return true;
            return false;
        }

        public static bool IsSavedSuccessfullyRedirect(this RedirectResult result)
        {
            if (result.Url.Contains(CustomizedControllerBase.SavedSuccessfullyFramgment) || result.Url.Contains(CustomizedControllerBase.SavedSuccessfullyActionName))
                return true;
            return false;

        }
    }

}