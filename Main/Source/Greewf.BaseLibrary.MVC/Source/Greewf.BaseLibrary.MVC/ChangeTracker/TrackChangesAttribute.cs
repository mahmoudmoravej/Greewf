using System;
using System.Web.Mvc;
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
                throw new Exception(string.Format("Your DbContext 'Controller.ContextManagerBase.ContextBase' should implements '{0}' interface in order to support using TrackChangesAttribute.", nameof(ISavingTracker)));

            if (controller.ContextManagerBase.ContextBase is ITransactionScopeAwareness == false)
                throw new Exception(string.Format("Your DbContext 'Controller.ContextManagerBase.ContextBase' should implements '{0}' interface in order to support using TrackChangesAttribute.", nameof(ITransactionScopeAwareness)));


            var savingTracker = controller.ContextManagerBase.ContextBase as ISavingTracker;
            savingTracker.OnChangesSaving += OnSavingChanges;

            var commitTracker = controller.ContextManagerBase.ContextBase as ITransactionScopeAwareness;
            commitTracker.OnChangesCommitted += OnChangesCommitted;//we should listen to commit to ensure that all changes are persisted
        }

        AuditingWidget auditingResult;
        private void OnSavingChanges(DbContext context)
        {
            auditingResult = ChangeTracker.Current.AuditContext(context);
        }

        private void OnChangesCommitted()
        {
            ChangeTracker.Current.SaveAuditing(auditingResult);
            auditingResult = null;
        }
    }

}