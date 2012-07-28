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
        private LogModelState logOnModelState;
        private LogResultType logOnResultType;
        private string[] exludeModelProperties;

        protected LogAttributeBase(int logId, Type logEnumType, LogModelState modelState = LogModelState.Always, LogResultType resultType = LogResultType.Always, string[] exludeModelProperties = null)
        {
            this.logId = logId;
            this.logEnumType = logEnumType;
            this.logOnModelState = modelState;
            this.logOnResultType = resultType;
            this.exludeModelProperties = exludeModelProperties;
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);
            if (LogProfileReader.Current.IsLogDisabled(logId, logEnumType)) return;

            var controller = filterContext.Controller as CustomizedControllerBase;
            if (controller == null) return;

            if (!IsModelValidToLog(controller)) return;
            if (!IsResultValidToLog(filterContext.Result)) return;

            var model = filterContext.Controller.ViewData.Model;
            var modelMetadata = filterContext.Controller.ViewData.ModelMetadata;

            var customizedLogDetails = controller.GetLogDetails(logId, model);
            if (customizedLogDetails != null)
            {
                model = customizedLogDetails;
                modelMetadata = null;
            }

            Logger.Current.Log(logId, logEnumType, model, modelMetadata, exludeModelProperties);

        }

        private bool IsResultValidToLog(ActionResult actionResult)
        {

            if (logOnResultType == LogResultType.Always)
                return true;
            else if ((logOnResultType & LogResultType.SuccessView) > 0 && actionResult is SuccessViewResult)
                return true;
            else if ((logOnResultType & LogResultType.View) > 0 && actionResult is ViewResultBase)
                return true;
            else if (actionResult is RedirectToRouteResult || actionResult is RedirectResult)
            {
                if ((logOnResultType & LogResultType.Redirect) > 0)
                    return true;
                else if ((logOnResultType & LogResultType.RedirectToSuccess) > 0)
                {
                    if ((actionResult is RedirectToRouteResult) && (actionResult as RedirectToRouteResult).IsSavedSuccessfullyRedirect())
                        return true;
                    if ((actionResult is RedirectToRouteResultEx) && (actionResult as RedirectToRouteResultEx).IsSavedSuccessfullyRedirect())
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
                case LogModelState.Always:
                    return true;
                case LogModelState.Valid:
                    if (controller.ViewData == null || controller.ViewData.ModelState == null) return false;
                    return controller.ViewData.ModelState.IsValid;
                case LogModelState.Invalid:
                    if (controller.ViewData == null || controller.ViewData.ModelState == null) return false;
                    return !controller.ViewData.ModelState.IsValid;
                default:
                    return true;
            }
        }


    }

    public enum LogModelState
    {
        Valid = 1,
        Invalid = 2,
        Always = Valid | Invalid
    }

    public enum LogResultType
    {
        View = 1,
        Redirect = 2,
        RedirectToSuccess = 4,
        SuccessView = 8,
        Always = View | Redirect | RedirectToSuccess | SuccessView
    }

}