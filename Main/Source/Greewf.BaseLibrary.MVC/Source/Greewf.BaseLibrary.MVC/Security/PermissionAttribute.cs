using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Authentication;
using System.Security;


namespace Greewf.BaseLibrary.MVC.Security
{
    public abstract class PermissionAttributeBase : ActionFilterAttribute
    {

        private readonly IEnumerable<long> permissions;
        private readonly long permissionObject;
        private readonly string permissionCategoryKeyParameterName;
        private readonly string entityKeyParameter;
        private bool anyCategory;

        protected PermissionAttributeBase(long permissionObject, long permissions, string entityKeyParameter = null, string permissionCategoryKeyParameterName = null)
        {
            this.permissionObject = permissionObject;
            this.permissions = new long[] { permissions };
            this.permissionCategoryKeyParameterName = permissionCategoryKeyParameterName;
            this.entityKeyParameter = entityKeyParameter;
        }

        protected PermissionAttributeBase(long permissionObject, IEnumerable<long> permissions, string entityKeyParameter = null, string permissionCategoryKeyParameterName = null)
        {
            this.permissionObject = permissionObject;
            this.permissions = permissions;
            this.permissionCategoryKeyParameterName = permissionCategoryKeyParameterName;
            this.entityKeyParameter = entityKeyParameter;
        }

        protected PermissionAttributeBase(long permissionObject, IEnumerable<long> permissions, bool anyCategory)
        {
            this.permissionObject = permissionObject;
            this.permissions = permissions;
            this.anyCategory = anyCategory;
            permissionCategoryKeyParameterName = null;
            if (anyCategory == false)
            {
                throw new Exception("'anyCategory' parameter of 'PermissionAttributeBase' can only set to be 'true'. For 'false' behavior use other constructor signatures.");
            }
        }


        private object _parameterCategoryKey = null;
        private string _parameterCategoryKeyName = null;
        private object _entityKey = null;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var currentUser = CurrentUserBase.GetActiveInstance();
            var controller = filterContext.Controller as CustomizedControllerBase;
            if (controller == null) return;
            if (permissionCategoryKeyParameterName != null)
                _parameterCategoryKey = filterContext.ActionParameters[permissionCategoryKeyParameterName];//todo : test to have value in ActionExtectued method too.
            if (entityKeyParameter != null)
                _entityKey = filterContext.ActionParameters[entityKeyParameter];//todo : test to have value in ActionExtectued method too.

            object categoryKey = GetPerimssionCategoryKey(currentUser, controller);

            foreach (long per in permissions)
            {
                bool hasNoPermission;

                if (anyCategory)
                    hasNoPermission = currentUser.HasAnyCategoryPermission(permissionObject, per, null) != true;
                else
                    hasNoPermission = currentUser.HasPermission(permissionObject, per, null, categoryKey) != true;

                if (hasNoPermission)
                    throw new SecurityException( permissionObject);
            }

        }

        private object GetPerimssionCategoryKey(CurrentUserBase currentUser, CustomizedControllerBase controller)
        {
            // در سه حالت به دنبال رسته مربوط به اجازه می گردد
            // 1- پارامتر تابع
            // 2- در کنترلر توسط تابع مربوطه
            // 3- در CommonUserBase
            // کاربر می تواند بسته به نیازش یکی از این روش ها را انتخاب کند

            object result = null;
            if (_parameterCategoryKey != null)
                result = _parameterCategoryKey;
            else
            {
                result = controller.GetPermissionCategoryKey(permissionObject, this.permissions, _entityKey, entityKeyParameter);
                if (result == null)
                    result = currentUser.GetPermissionCategoryKey(permissionObject, this.permissions, _entityKey, entityKeyParameter);
            }
            return result;
        }




        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            var controller = filterContext.Controller as CustomizedControllerBase;
            if (controller == null) return;
            if (filterContext.Result is RedirectResult || filterContext.Result is RedirectToRouteResult) return;//in redirecting to another page(when each page/controller is responsible for its actions security) we don't need to check the permissions

            var model = filterContext.Controller.ViewData.Model;
            if (model != null)
            {
                var limiterModel = controller.GetModelLimiterFunctions(model);
                if (limiterModel != null)
                {
                    bool? andPartResult = null, orPartResult = null;
                    List<string> errorMessages = new List<string>();
                    var currentUser = CurrentUserBase.GetActiveInstance();
                    object categoryKey = GetPerimssionCategoryKey(currentUser, controller);

                    foreach (long per in permissions)
                    {

                        foreach (var limiter in limiterModel.LimiterFunctions/*.OrderBy(o => !o.IsAndPart) : no need anymore? */)
                        {
                            if (limiter == null) continue;
                            bool? x;
                            if (anyCategory)
                                x = currentUser.HasAnyCategoryPermission(permissionObject, per, limiter);
                            else
                                x = currentUser.HasPermission(permissionObject, per, limiter, categoryKey);

                            if (limiter.IsAndPart)
                                andPartResult = (andPartResult ?? true) && (x ?? true);
                            else//or base
                                orPartResult = (orPartResult ?? false) || (x ?? false);

                            //error message
                            if (x == false)
                            {//TODO: is it corrent in OrPart case? when x==false but the whole result may be true finally ?!
                                string msg = limiter.ErrorMessage == null ? null : limiter.ErrorMessage();
                                if (!string.IsNullOrWhiteSpace(msg)) errorMessages.Add(msg);
                            }

                            if (andPartResult.HasValue && andPartResult == false)
                                break;

                        }

                        if (andPartResult.HasValue && andPartResult == false)
                            break;
                    }

                    bool result = (andPartResult ?? true) && (orPartResult ?? true);
                    if (result == false)
                        throw new SecurityException(permissionObject, errorMessages.ToArray());

                }
            }


        }


    }
}