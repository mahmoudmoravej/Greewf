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


        protected internal abstract override object GetPermissionCategoryKey(long permissionObject, IEnumerable<long> permissions, object entityKey, string entityKeyParameterName);
    }

}