//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using System.Security.Authentication;
//using System.Security;


//namespace Greewf.BaseLibrary.MVC.Logging
//{
//    public abstract class LogAttributeBase : ActionFilterAttribute
//    {

//        private readonly IEnumerable<long> permissions;
//        private readonly long permissionObject;

//        protected LogAttributeBase(long permissionObject, long permissions)
//        {
//            this.permissionObject = permissionObject;
//            this.permissions = new long[] { permissions };
//        }


//        protected LogAttributeBase(long permissionObject, IEnumerable<long> permissions)
//        {
//            this.permissionObject = permissionObject;
//            this.permissions = permissions;
//        }

//        public override void OnActionExecuting(ActionExecutingContext filterContext)
//        {
//            foreach (long per in permissions)
//            {
//                if (CurrentUserBase.GetActiveInstance().HasPermission(permissionObject, per) != true)
//                    throw new SecurityException(permissionObject);
//            }

//        }


//        public override void OnActionExecuted(ActionExecutedContext filterContext)
//        {
//            base.OnActionExecuted(filterContext);
//            var controller = filterContext.Controller as CustomizedControllerBase;
//            if (controller == null) return;

//            var model = filterContext.Controller.ViewData.Model;
//            if (model != null)
//            {
//                var limiterModel = controller.GetModelLimiterFunctions(model);
//                if (limiterModel != null)
//                {
//                    bool? andPartResult = null, orPartResult = null;
//                    List<string> errorMessages = new List<string>();

//                    foreach (long per in permissions)
//                    {

//                        foreach (var limiter in limiterModel.LimiterFunctions/*.OrderBy(o => !o.IsAndPart) : no need anymore? */)
//                        {
//                            if (limiter == null) continue;
//                            bool? x = CurrentUserBase.GetActiveInstance().HasPermission(permissionObject, per, limiter);

//                            if (limiter.IsAndPart)
//                                andPartResult = (andPartResult ?? true) && (x ?? true);
//                            else//or base
//                                orPartResult = (orPartResult ?? false) || (x ?? false);

//                            //error message
//                            if (x == false)
//                            {//TODO: is it corrent in OrPart case? when x==false but the whole result may be true finally ?!
//                                string msg = limiter.ErrorMessage == null ? null : limiter.ErrorMessage();
//                                if (!string.IsNullOrWhiteSpace(msg)) errorMessages.Add(msg);
//                            }

//                            if (andPartResult.HasValue && andPartResult == false)
//                                break;

//                        }

//                        if (andPartResult.HasValue && andPartResult == false)
//                            break;
//                    }

//                    bool result = (andPartResult ?? true) && (orPartResult ?? true);
//                    if (result == false)
//                        throw new SecurityException(permissionObject, errorMessages.ToArray());

//                }
//            }


//        }


//    }
//}