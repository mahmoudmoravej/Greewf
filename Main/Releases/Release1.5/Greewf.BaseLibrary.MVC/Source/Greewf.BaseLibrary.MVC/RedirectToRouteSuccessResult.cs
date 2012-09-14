using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace Greewf.BaseLibrary.MVC
{
    public class RedirectToRouteSuccessResult : RedirectToRouteResult
    {
        public RedirectToRouteSuccessResult(RouteValueDictionary routeValues)
            : base(routeValues)
        {
        }

        public RedirectToRouteSuccessResult(string routeName, RouteValueDictionary routeValues)
            : base(routeName, routeValues)
        {

        }

        public RedirectToRouteSuccessResult(string routeName, RouteValueDictionary routeValues, bool permanent)
            : base(routeName, routeValues, permanent)
        {

        }

        public RedirectToRouteSuccessResult(RedirectToRouteResult baseResult)
            : base(baseResult.RouteName, baseResult.RouteValues, baseResult.Permanent)
        {
        }
    }
}
