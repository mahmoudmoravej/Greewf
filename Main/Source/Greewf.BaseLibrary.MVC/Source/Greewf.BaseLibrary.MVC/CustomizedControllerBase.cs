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
            url = EnsureWindowFlag(url);
            url = EnsureSaveFlag(url, setSaveSuccesfullyFlag);

            return base.Redirect(url);

        }

        private string EnsureSaveFlag(string url, bool setSaveSuccesfullyFlag)
        {
            if (setSaveSuccesfullyFlag == false) return url;
            if (url.Contains("#"))
                return url + ";" + SavedSuccessfullyFramgment;//surely the hash segment is placed at the end of url , so we add our string to it simply
            else
                return url + "#" + SavedSuccessfullyFramgment;
        }

        private RedirectToRouteResult EnsureSaveFlag(RedirectToRouteResult result, bool setSaveSuccesfullyFlag)
        {
            if (setSaveSuccesfullyFlag == false) return result;
            return result.AddFragment(SavedSuccessfullyFramgment);
        }

        private string CheckRedirectAddress(string url, bool setSaveSuccesfullyFlag)
        {
            if (IsCurrentRequestRunInWindow && setSaveSuccesfullyFlag)//when the current request is in window and it is savedsucessfully, we redirect it to home/SavedSuccessfully page automatically
                return string.Format(SavedSuccessfullyUrlFormat, url);
            return url;
        }

        private RedirectToRouteResult CheckRedirectAddress(RedirectToRouteResult result, bool setSaveSuccesfullyFlag)
        {
            if (IsCurrentRequestRunInWindow && setSaveSuccesfullyFlag)//when the current request is in window and it is savedsucessfully, we redirect it to home/SavedSuccessfully page automatically
                return base.RedirectToAction(SavedSuccessfullyActionName, SavedSuccessfullyControllerName, new { url = result.ToString(this.ControllerContext) });
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
            EnsureWindowFlag(result.RouteValues);
            result = EnsureSaveFlag(result, setSaveSuccesfullyFlag);

            return result;
        }

        protected override RedirectToRouteResult RedirectToRoute(string routeName, System.Web.Routing.RouteValueDictionary routeValues)
        {
            EnsureWindowFlag(routeValues);
            return base.RedirectToRoute(routeName, routeValues);
        }

        private void EnsureWindowFlag(System.Web.Routing.RouteValueDictionary routeValues)
        {
            //this method should also ensures Puremode and Simplemode too
            EnsureFlag(routeValues, IsCurrentRequestRunInWindow, "iswindow");
            EnsureFlag(routeValues, Request.QueryString.AllKeys.Contains("puremode"), "puremode");
            EnsureFlag(routeValues, Request.QueryString.AllKeys.Contains("simplemode"), "simplemode");
            EnsureFlag(routeValues, Request.QueryString.AllKeys.Contains("includeUrlInContent"), "includeUrlInContent");

        }

        private string EnsureWindowFlag(string url)
        {
            //this method should also ensures Puremode and Simplemode too
            url = EnsureFlag(url, IsCurrentRequestRunInWindow, "iswindow");
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
        protected ActionResult ResponsiveJson(ResponsiveJsonType responseType, ModelStateDictionary modelState, object modelToLog)
        {
            ViewData.Model = modelToLog;
            var responsiveJsonResult = new ResponsiveJsonResult(responseType, modelState);

            return GetAppropriateJsonResult(responsiveJsonResult);
        }

        private ActionResult GetAppropriateJsonResult(ResponsiveJsonResult responsiveJsonResult)
        {
            if (IsCurrentRequestRunInWindow)
                return responsiveJsonResult;
            else
                return View("~/Views/Home/_MessageView.cshtml", responsiveJsonResult);
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
            Logger.Current.Log(logId, model, exludeModelProperties);
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

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">The main related Entity the current controller should work on</typeparam>
    /// <typeparam name="VM">The default ViewModel the current controller should work on</typeparam>
    /// <typeparam name="Y">The main Context Manager</typeparam>
    /// <typeparam name="Z">UnitOfRepository Interface class</typeparam>    
    public abstract class CustomizedControllerBase<T, VM, Y, Z> : CustomizedControllerBase
        where T : class ,new()
        where Y : ContextManagerBase
        where VM : class , new()
    {
        protected Y ContextManager = null;

        public CustomizedControllerBase()
        {
            CreateInstances(out ContextManager, out  _uoR);
            ContextManagerBase = ContextManager;
        }

        private Z _uoR;
        protected Z UoR
        {
            get
            {
                return _uoR;
            }
        }

        protected abstract void CreateInstances(out Y contextManagerInstance, out  Z unitOfRepositoriesInstance);

        protected virtual SensetiveFields<VM> GetSensitiveDataFields(T oldEntity, ActionType actionType, bool? isHttpPost = null)
        {
            return null;
        }

        protected bool TryUpdateModel<M>(M model, T oldEntity, ActionType actionType, bool? isPOST = null) where M : class, new()
        {
            var sensitiveData = GetSensitiveDataFields(oldEntity, actionType, isPOST);
            return TryUpdateModel(model, null, null, sensitiveData == null ? null : sensitiveData.ToStringArray());
        }

        protected void UpdateModel<M>(M model, T oldEntity, ActionType actionType, bool? isPOST = null) where M : class, new()
        {
            var sensitiveData = GetSensitiveDataFields(oldEntity, actionType, isPOST);
            UpdateModel(model, null, null, sensitiveData == null ? null : sensitiveData.ToStringArray());
        }


        //protected virtual SensetiveFields<M> GetSensitiveDataFields<M>(T oldEntity, ActionType actionType, bool? isHttpPost = null) where M : class, new()
        //{
        //    return null;
        //}

        //protected bool TryUpdateModel<M>(M model, T oldEntity, ActionType actionType, bool? isPOST = null) where M : class, new()
        //{
        //    var sensitiveData = GetSensitiveDataFields<M>(oldEntity, actionType, isPOST);
        //    return TryUpdateModel(model, null, null, sensitiveData == null ? null : sensitiveData.ToStringArray());
        //}

    }


    public class RedirectToRouteResultEx : RedirectToRouteResult
    {

        public RedirectToRouteResultEx(RouteValueDictionary values)
            : base(values)
        {
        }

        public RedirectToRouteResultEx(string routeName, RouteValueDictionary values)
            : base(routeName, values)
        {
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var destination = new StringBuilder();

            var helper = new UrlHelper(context.RequestContext);
            destination.Append(helper.RouteUrl(RouteName, RouteValues));

            //Add href fragment if set
            if (!string.IsNullOrEmpty(Fragment))
            {
                destination.AppendFormat("#{0}", Fragment);
            }

            context.HttpContext.Response.Redirect(destination.ToString(), false);
        }

        public string Fragment { get; set; }

    }

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

    public class SuccessViewResult : ViewResult
    {
    }

    public class ModelPermissionLimiters
    {
        public PermissionLimiterBase[] LimiterFunctions { get; set; }
    }

    public enum ActionType
    {
        Create,
        Edit,
        Delete,
        Search,
        Index,
        View
    }

    public class SensetiveFields<T> : List<Expression<Func<T, object>>>
    {
        public new SensetiveFields<T> Add(Expression<Func<T, object>> field)
        {
            base.Add(field);
            return this;
        }

        public string[] ToStringArray()
        {
            return this.Select(o =>
            {
                if (o.Body is UnaryExpression)
                    return ((MemberExpression)((UnaryExpression)o.Body).Operand).Member.Name;
                else
                    return ExpressionHelper.GetExpressionText(o);
            }).ToArray();
        }

    }


}