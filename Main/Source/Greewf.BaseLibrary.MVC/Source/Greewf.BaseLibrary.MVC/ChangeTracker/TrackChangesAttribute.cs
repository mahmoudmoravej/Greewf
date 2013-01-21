using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Authentication;
using System.Security;
using Greewf.BaseLibrary.MVC.Logging.LogContext;
using Greewf.BaseLibrary.MVC;
using System.Data.Entity;
using Greewf.BaseLibrary.Repositories;


namespace Greewf.BaseLibrary.MVC.ChangeTracker
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public abstract class TrackChangesAttributeBase : ActionFilterAttribute
    {

        protected TrackChangesAttributeBase()
        {
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (ChangeTracker.Current.AttributesDisabled) return;
            base.OnActionExecuting(filterContext);
            var controller = filterContext.Controller as CustomizedControllerBase;
            if (controller == null || controller.ContextManagerBase == null) return;

            if (controller.ContextManagerBase.ContextBase is ISavingTracker == false)
                throw new Exception(string.Format("Your DbContext 'Controller.ContextManagerBase.ContextBase' should implements '{0}' interface in order to support using TrackChangesAttribute.", typeof(ISavingTracker).ToString()));

            var context = controller.ContextManagerBase.ContextBase as ISavingTracker;
            context.OnSavingChanges += OnSavingChanges;
            context.OnSavedChanges += OnSavedChanges;
        }

        AuditingWidget auditingResult;
        private void OnSavingChanges(DbContext context)
        {
            auditingResult = ChangeTracker.Current.AuditContext(context);
        }

        private void OnSavedChanges(DbContext context)
        {
            ChangeTracker.Current.SaveAuditing(auditingResult);
            auditingResult = null;
        }
    }

}