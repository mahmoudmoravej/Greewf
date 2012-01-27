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
    public abstract class LogAttributeBase : ActionFilterAttribute
    {

        private readonly int logId;
        private readonly Type logEnumType;
        private bool logOnInvalidModel;
        private string[] exludeModelProperties;

        protected LogAttributeBase(int logId, Type logEnumType, bool logOnInvalidModel = false, string[] exludeModelProperties = null)
        {
            this.logId = logId;
            this.logEnumType = logEnumType;
            this.logOnInvalidModel = logOnInvalidModel;
            this.exludeModelProperties = exludeModelProperties;
        }   

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            var controller = filterContext.Controller as CustomizedControllerBase;
            if (controller == null) return;
            if (!controller.ViewData.ModelState.IsValid && !logOnInvalidModel) return;

            var model = filterContext.Controller.ViewData.Model;
            var modelMetaData = filterContext.Controller.ViewData.ModelMetadata;

            Logger.Current.Log(logId, logEnumType, model, modelMetaData, exludeModelProperties);

            //var limiterModel = controller.GetModelLimiterFunctions(model);
            //if (limiterModel != null)
            //{
            //    bool? andPartResult = null, orPartResult = null;
            //    List<string> errorMessages = new List<string>();

            //    foreach (long per in logId)
            //    {

            //        foreach (var limiter in limiterModel.LimiterFunctions/*.OrderBy(o => !o.IsAndPart) : no need anymore? */)
            //        {
            //            if (limiter == null) continue;
            //            bool? x = CurrentUserBase.GetActiveInstance().HasPermission(permissionObject, per, limiter);

            //            if (limiter.IsAndPart)
            //                andPartResult = (andPartResult ?? true) && (x ?? true);
            //            else//or base
            //                orPartResult = (orPartResult ?? false) || (x ?? false);

            //            //error message
            //            if (x == false)
            //            {//TODO: is it corrent in OrPart case? when x==false but the whole result may be true finally ?!
            //                string msg = limiter.ErrorMessage == null ? null : limiter.ErrorMessage();
            //                if (!string.IsNullOrWhiteSpace(msg)) errorMessages.Add(msg);
            //            }

            //            if (andPartResult.HasValue && andPartResult == false)
            //                break;

            //        }

            //        if (andPartResult.HasValue && andPartResult == false)
            //            break;
            //    }

            //    bool result = (andPartResult ?? true) && (orPartResult ?? true);
            //    if (result == false)
            //        throw new SecurityException(permissionObject, errorMessages.ToArray());

            //}
            //  }


        }


    }
}