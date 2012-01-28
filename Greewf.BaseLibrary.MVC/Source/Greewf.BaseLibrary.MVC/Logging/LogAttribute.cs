using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Authentication;
using System.Security;
using Greewf.BaseLibrary.MVC.Logging.LogContext;


namespace Greewf.BaseLibrary.MVC.Logging
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public abstract class LogAttributeBase : ActionFilterAttribute
    {

        private readonly int logId;
        private readonly Type logEnumType;
        private ModelState logOnModelState;
        private ResultType logOnResultType;
        private string[] exludeModelProperties;

        protected LogAttributeBase(int logId, Type logEnumType, ModelState modelState = ModelState.Always, ResultType resultType = ResultType.Always, string[] exludeModelProperties = null)
        {
            this.logId = logId;
            this.logEnumType = logEnumType;
            this.logOnModelState = modelState;
            this.logOnResultType = resultType;
            this.exludeModelProperties = exludeModelProperties;
        }

        //public override void OnActionExecuted(ActionExecutedContext filterContext)
        //{
        //    base.OnActionExecuted(filterContext);
        //    var controller = filterContext.Controller as CustomizedControllerBase;
        //    if (controller == null) return;
        //    if (!controller.ViewData.ModelState.IsValid && !logOnInvalidModel) return;

        //    var model = filterContext.Controller.ViewData.Model;
        //    var modelMetaData = filterContext.Controller.ViewData.ModelMetadata;

        //    Logger.Current.Log(logId, logEnumType, model, modelMetaData, exludeModelProperties);

        //}

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);

            var controller = filterContext.Controller as CustomizedControllerBase;
            if (controller == null) return;

            if (!IsModelValidToLog(controller)) return;
            if (!IsResultValidToLog(filterContext.Result)) return;

            var model = filterContext.Controller.ViewData.Model;
            var modelMetadata = filterContext.Controller.ViewData.ModelMetadata;

            Logger.Current.Log(logId, logEnumType, model, modelMetadata, exludeModelProperties);

        }

        private bool IsResultValidToLog(ActionResult actionResult)
        {

            if ((logOnResultType | ResultType.Always) > 0)
                return true;
            else if ((logOnResultType | ResultType.View) > 0 && actionResult is ViewResultBase)
                return true;
            else if (actionResult is RedirectToRouteResult || actionResult is RedirectResult)
            {
                if ((logOnResultType | ResultType.Redirect) > 0)
                    return true;
                else if ((logOnResultType | ResultType.RedirectToSuccess) > 0)
                {
                    if ((actionResult is RedirectToRouteResult) && (actionResult as RedirectToRouteResult).IsSavedSuccessfullyRedirect())
                        return true;
                    if ((actionResult is RedirectResult) && (actionResult as RedirectResult).IsSavedSuccessfullyRedirect())
                        return true;

                }
            }

            return false;
        }

        private bool IsModelValidToLog(CustomizedControllerBase controller)
        {
            switch (logOnModelState)
            {
                case ModelState.Always:
                    return true;
                case ModelState.Valid:
                    if (controller.ViewData == null || controller.ViewData.ModelState == null) return false;
                    return controller.ViewData.ModelState.IsValid;
                case ModelState.Invalid:
                    if (controller.ViewData == null || controller.ViewData.ModelState == null) return false;
                    return !controller.ViewData.ModelState.IsValid;
                default:
                    return true;
            }
        }

        public enum ModelState
        {
            Valid = 1,
            Invalid = 2,
            Always = Valid | Invalid
        }

        public enum ResultType
        {
            View = 1,
            Redirect = 2,
            RedirectToSuccess = 4,
            Always = View | Redirect | RedirectToSuccess
        }



    }


}