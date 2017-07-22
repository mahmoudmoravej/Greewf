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
using Greewf.BaseLibrary.Logging;

namespace Greewf.BaseLibrary.MVC
{
    public abstract class CustomizedControllerBase : Controller
    {
        private const string SavedSuccessfullyUrlFormat = "~/home/SavedSuccessfully?url={0}";
        private const string SavedSuccessfullyControllerName = "home";
        internal const string SavedSuccessfullyActionName = "SavedSuccessfully";
        internal const string SavedSuccessfullyFramgment = "successfullysaved";

        private new RedirectResult Redirect(string url)
        {
            return Redirect(url, false, null);
        }

        protected RedirectResult Redirect(string url, object model)
        {
            return Redirect(url, false, model);
        }

        protected RedirectResult Redirect(string url, bool setSaveSuccesfullyFlag, object model)
        {
            ViewData.Model = model;
            url = CheckRedirectAddress(url, setSaveSuccesfullyFlag);
            url = EnsureLayoutFlags(url);
            url = EnsureSaveFlag(url, setSaveSuccesfullyFlag);

            return base.Redirect(url);

        }

        private string EnsureSaveFlag(string url, bool setSaveSuccesfullyFlag)
        {
            if (setSaveSuccesfullyFlag == false) return url;
            Response.AddHeader(SavedSuccessfullyFramgment, "true");//surely this line is not usefull! because the redirect action in clinet doesn't resend the header. so we can remove this line as we don't use it anywhere too
            if (url.Contains("#"))
                return url + ";" + SavedSuccessfullyFramgment;//surely the hash segment is placed at the end of url , so we add our string to it simply
            else
                return url + "#" + SavedSuccessfullyFramgment;
        }

        private RedirectToRouteResult EnsureSaveFlag(RedirectToRouteResult result, bool setSaveSuccesfullyFlag)
        {
            if (setSaveSuccesfullyFlag == false) return result;
            Response.AddHeader(SavedSuccessfullyFramgment, "true");//surely this line is not usefull! because the redirect action in clinet doesn't resend the header. so we can remove this line as we don't use it anywhere too
            return result.AddFragment(SavedSuccessfullyFramgment);
        }

        private string CheckRedirectAddress(string url, bool setSaveSuccesfullyFlag)
        {
            bool ispopup = IsCurrentRequestRunInWindow;
            if (ispopup && setSaveSuccesfullyFlag)//when the current request is in window and it is savedsucessfully, we redirect it to home/SavedSuccessfully page automatically
                return string.Format(SavedSuccessfullyUrlFormat, url);
            else if (!ispopup && setSaveSuccesfullyFlag)//in a Single page or a tab
                return string.Format(SavedSuccessfullyUrlFormat, url) + "&forceToPassedUrl=1";

            return url;
        }

        private RedirectToRouteResult CheckRedirectAddress(RedirectToRouteResult result, bool setSaveSuccesfullyFlag)
        {
            bool ispopup = IsCurrentRequestRunInWindow;
            if (ispopup && setSaveSuccesfullyFlag)//when the current request is in window and it is savedsucessfully, we redirect it to home/SavedSuccessfully page automatically
                return base.RedirectToAction(SavedSuccessfullyActionName, SavedSuccessfullyControllerName, new { url = result.ToString(this.ControllerContext) });
            else if (!ispopup && setSaveSuccesfullyFlag)//in a Single page or a tab 
                if (!(result.RouteValues["controller"].ToString().ToLower() == SavedSuccessfullyControllerName.ToLower() ||
                    result.RouteValues["action"].ToString().ToLower() == SavedSuccessfullyActionName.ToLower()))//and the request is different from SavedSuccessfullyActionName
                    return base.RedirectToAction(SavedSuccessfullyActionName, SavedSuccessfullyControllerName, new { url = result.ToString(this.ControllerContext), forceToPassedUrl = 1 });

            return result;
        }


        protected override RedirectToRouteResult RedirectToAction(string actionName, string controllerName, System.Web.Routing.RouteValueDictionary routeValues)
        {
            return this.RedirectToAction(actionName, controllerName, routeValues, false);
        }

        protected RedirectToRouteResult RedirectToSuccessAction(object model)
        {
            ViewData.Model = model;
            var result = base.RedirectToAction(SavedSuccessfullyActionName, SavedSuccessfullyControllerName);
            return this.CorrectRedirectToRouteResult(result, true);
        }

        protected RedirectToRouteResult RedirectToSuccessAction(string actionName, string controllerName, System.Web.Routing.RouteValueDictionary routeValues, object model)
        {
            ViewData.Model = model;
            var result = base.RedirectToAction(actionName, controllerName, routeValues);
            return this.CorrectRedirectToRouteResult(result, true);
        }

        protected RedirectToRouteResult RedirectToSuccessAction(string actionName, string controllerName, object routeValues, object model)
        {
            ViewData.Model = model;
            var result = base.RedirectToAction(actionName, controllerName, routeValues);
            return this.CorrectRedirectToRouteResult(result, true);
        }

        protected RedirectToRouteResult RedirectToSuccessAction(string actionName, object routeValues, object model)
        {
            ViewData.Model = model;
            var result = base.RedirectToAction(actionName, routeValues);
            return this.CorrectRedirectToRouteResult(result, true);
        }

        protected RedirectToRouteResult RedirectToAction(string actionName, string controllerName, System.Web.Routing.RouteValueDictionary routeValues, bool setSaveSuccesfullyFlag)
        {
            var result = base.RedirectToAction(actionName, controllerName, routeValues);
            return CorrectRedirectToRouteResult(result, setSaveSuccesfullyFlag);

        }

        private RedirectToRouteResult CorrectRedirectToRouteResult(RedirectToRouteResult result, bool setSaveSuccesfullyFlag)
        {

            result = CheckRedirectAddress(result, setSaveSuccesfullyFlag);
            EnsureLayoutFlags(result.RouteValues);
            result = EnsureSaveFlag(result, setSaveSuccesfullyFlag);

            return result;
        }

        protected override RedirectToRouteResult RedirectToRoute(string routeName, System.Web.Routing.RouteValueDictionary routeValues)
        {
            EnsureLayoutFlags(routeValues);
            return base.RedirectToRoute(routeName, routeValues);
        }

        private void EnsureLayoutFlags(System.Web.Routing.RouteValueDictionary routeValues)
        {
            //this method should also ensures Puremode and Simplemode too
            EnsureFlag(routeValues, IsCurrentRequestRunInWindow, "iswindow");
            EnsureFlag(routeValues, Request.QueryString.AllKeys.Contains("istab"), "istab");
            EnsureFlag(routeValues, Request.QueryString.AllKeys.Contains("puremode"), "puremode");
            EnsureFlag(routeValues, Request.QueryString.AllKeys.Contains("simplemode"), "simplemode");
            EnsureFlag(routeValues, Request.QueryString.AllKeys.Contains("includeUrlInContent"), "includeUrlInContent");

        }

        private string EnsureLayoutFlags(string url)
        {
            //this method should also ensures Puremode and Simplemode too
            url = EnsureFlag(url, IsCurrentRequestRunInWindow, "iswindow");
            url = EnsureFlag(url, Request.QueryString.AllKeys.Contains("istab"), "istab");
            url = EnsureFlag(url, Request.QueryString.AllKeys.Contains("puremode"), "puremode");
            url = EnsureFlag(url, Request.QueryString.AllKeys.Contains("simplemode"), "simplemode");
            url = EnsureFlag(url, Request.QueryString.AllKeys.Contains("includeUrlInContent"), "includeUrlInContent");


            return url;
        }

        private string EnsureFlag(string url, bool applyFlag, string flag)
        {
            if (applyFlag && url.IndexOf(flag) == -1)
            {
                if (url.IndexOf("?") == -1)
                    return url + "?" + flag + "=1";
                else
                    return url + "&" + flag + "=1";
            }
            return url;
        }

        private void EnsureFlag(RouteValueDictionary routeValues, bool applyFlag, string flag)
        {
            if (applyFlag && !routeValues.Keys.Contains(flag))
                routeValues.Add(flag, 1);
        }


        private bool IsCurrentRequestRunInWindow
        {
            get
            {
                return Request.QueryString.AllKeys.Contains("iswindow");
            }
        }


        protected JsonResult Json(object data, object model)
        {
            ViewData.Model = model;
            return base.Json(data);
        }

        /// <summary>
        /// توجه : اگر نمیخواهید چیزی لاگ شود modelToLog را null بفرستید
        /// </summary>
        /// <param name="responseType"></param>
        /// <param name="message"></param>
        /// <param name="modelToLog">مدلی که می خواهید لاگ شود. اگر نمی خواهید چیزی لاگ شود آنرا null بفرستید</param>
        /// <returns></returns>
        protected ActionResult ResponsiveJson(ResponsiveJsonType responseType, string message, object modelToLog)
        {
            ViewData.Model = modelToLog;
            var responsiveJsonResult = new ResponsiveJsonResult(responseType, message);

            return GetAppropriateJsonResult(responsiveJsonResult);
        }

        /// <summary>
        /// توجه : اگر نمیخواهید چیزی لاگ شود modelToLog را null بفرستید
        /// </summary>
        /// <param name="responseType"></param>
        /// <param name="message"></param>
        /// <param name="modelToLog">مدلی که می خواهید لاگ شود. اگر نمی خواهید چیزی لاگ شود آنرا null بفرستید</param>
        /// <returns></returns>
        protected ActionResult ResponsiveJson(ResponsiveJsonType responseType, string message, string details, object modelToLog)
        {
            ViewData.Model = modelToLog;
            var responsiveJsonResult = new ResponsiveJsonResult(responseType, message, details);

            return GetAppropriateJsonResult(responsiveJsonResult);
        }

        /// <summary>
        /// توجه : اگر نمیخواهید چیزی لاگ شود modelToLog را null بفرستید
        /// </summary>
        /// <param name="modelState"></param>
        /// <param name="modelToLog">مدلی که می خواهید لاگ شود. اگر نمی خواهید چیزی لاگ شود آنرا null بفرستید</param>
        /// <returns></returns>
        protected ActionResult ResponsiveJson(ModelStateDictionary modelState, object modelToLog)
        {
            ViewData.Model = modelToLog;
            var responsiveJsonResult = new ResponsiveJsonResult(modelState);

            return GetAppropriateJsonResult(responsiveJsonResult);
        }

        /// <summary>
        /// توجه : اگر نمیخواهید چیزی لاگ شود modelToLog را null بفرستید
        /// </summary>
        /// <param name="modelState"></param>
        /// <param name="modelToLog">مدلی که می خواهید لاگ شود. اگر نمی خواهید چیزی لاگ شود آنرا null بفرستید</param>
        /// <returns></returns>
        protected ActionResult ResponsiveJson(ModelStateDictionary modelState, string details, object modelToLog)
        {
            ViewData.Model = modelToLog;
            var responsiveJsonResult = new ResponsiveJsonResult(modelState, details);

            return GetAppropriateJsonResult(responsiveJsonResult);
        }

        /// <summary>
        /// توجه : اگر نمیخواهید چیزی لاگ شود modelToLog را null بفرستید
        /// </summary>
        /// <param name="modelState"></param>
        /// <param name="modelToLog">مدلی که می خواهید لاگ شود. اگر نمی خواهید چیزی لاگ شود آنرا null بفرستید</param>
        /// <returns></returns>
        protected ActionResult ResponsiveJson(ResponsiveJsonType responseType, ModelStateDictionary modelState, object modelToLog)
        {
            ViewData.Model = modelToLog;
            var responsiveJsonResult = new ResponsiveJsonResult(responseType, modelState);

            return GetAppropriateJsonResult(responsiveJsonResult);
        }

        /// <summary>
        /// توجه : اگر نمیخواهید چیزی لاگ شود modelToLog را null بفرستید
        /// </summary>
        /// <param name="modelState"></param>
        /// <param name="modelToLog">مدلی که می خواهید لاگ شود. اگر نمی خواهید چیزی لاگ شود آنرا null بفرستید</param>
        /// <returns></returns>
        protected ActionResult ResponsiveJson(ResponsiveJsonType responseType, ModelStateDictionary modelState, string details, object modelToLog)
        {
            ViewData.Model = modelToLog;
            var responsiveJsonResult = new ResponsiveJsonResult(responseType, modelState, details);

            return GetAppropriateJsonResult(responsiveJsonResult);
        }

        private ActionResult GetAppropriateJsonResult(ResponsiveJsonResult responsiveJsonResult)
        {
            if (IsCurrentRequestRunInWindow || IsAjaxCall())
                return responsiveJsonResult;
            else
                return View("~/Views/Home/_MessageView.cshtml", responsiveJsonResult);
        }

        private bool IsAjaxCall()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";//Refrence : http://en.wikipedia.org/wiki/List_of_HTTP_header_fields
        }

        protected SuccessViewResult SuccessView(string viewName, object model)
        {
            ViewData.Model = model;
            return new SuccessViewResult() { TempData = this.TempData, ViewName = viewName, ViewData = this.ViewData };
        }


        protected internal virtual ModelPermissionLimiters GetModelLimiterFunctions(dynamic model)
        {
            //TODO : make some conventions on it (for example : UserName,CreatorOwner,CreatedByUserName,OwnerUserName,CreatorUserName are good to automatically undrestand)
            //var modelType = (model as object).GetType();
            //if (modelType.prop .UserName != null)
            //    return model.UserName;
            //else if (model.CreatedByUserName != null)
            //    return model.CreatedByUserName;

            return null;
        }

        protected internal virtual Dictionary<string, string> GetLogDetails(int logPointId, dynamic model)
        {
            return null;
        }

        protected void Log<T>(T logId, object model, string[] exludeModelProperties = null) where T : struct
        {
            HttpWebLogger.Current.Log(logId, model, exludeModelProperties);
        }

        public ContextManagerBase ContextManagerBase
        {
            get;
            protected set;
        }

        /// <summary>
        /// فقط در صفحه Home فراخوانی شود
        /// </summary>
        /// <returns></returns>
        protected ActionResult GetAccessDeniedPage()
        {
            ViewBag.ErrorMessages = Session["ErrorMessages"] ?? new string[] { };
            Session["ErrorMessages"] = null;

            Response.TrySkipIisCustomErrors = true;//we need it for IIS 7.0 (on win 2008 R2)
            Response.StatusCode = 500;//to make ajax call enable getting it through onError event
            Response.AddHeader("GreewfAccessDeniedPage", "true"); //to help ajax onError event to distinguish between regular content or custom error page content.

            return View();
        }

        protected ActionResult GetCustomErrorPage()
        {
            ViewBag.ErrorMessages = Session["ErrorMessages"] ?? new string[] { };
            Session["ErrorMessages"] = null;

            Response.TrySkipIisCustomErrors = true;//we need it for IIS 7.0 (on win 2008 R2)
            Response.StatusCode = 500;//to make ajax call enable getting it through onError event
            Response.AddHeader("GreewfCustomErrorPage", "true"); //to help ajax onError event to distinguish between regular content or custom error page content.

            return View();

        }

        public ActionResult GetSavedSuccessfullyPage()
        {
            return View();
        }

        protected internal abstract object GetPermissionCategoryKey(long permissionObject, IEnumerable<long> permissions, object entityKey, string entityKeyParameterName);

    }


}