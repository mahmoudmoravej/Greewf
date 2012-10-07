using System;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using System.Web.Profile;
using System.Collections.Generic;
using System.Configuration;
using Greewf.BaseLibrary.MVC.Security;


namespace Greewf.BaseLibrary.MVC
{

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="P">PermissionEntity enum</typeparam>
    /// <typeparam name="PC">PermissionCoordinator</typeparam>
    /// <typeparam name="PL">PermissionLimiter</typeparam>
    public abstract class CurrentUserBase<P, PC, PL> : CurrentUserBase
        where P : struct
        where PC : PermissionCoordinatorBase<P>
        where PL : PermissionLimiterBase<P, PC>, new()
    {


        protected override abstract string EnterpriseAdminUsername { get; }

        protected internal override object GetPermissionCategoryKey(long permissionObject)
        {
            return null;
        }


        #region Permissions

        public bool HasPermission<T>(T permissions, string itemCreatorUserName = null) where T : struct
        {
            var entityItem = PermissionCoordinator.GetRelatedPermissionItem(typeof(T));
            return HasPermission(entityItem, Convert.ToInt64(permissions), itemCreatorUserName) == true;
        }

        /// <summary>
        /// از ارسال اجازه های ترکیبی به این پارامتر خودداری شود
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public bool HasFullPermissionOf<T>(T permissions) where T : struct
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated) return false;
            return UserPermissionHelper.HasFullPermissionOf<T>(permissions);
        }

        /// <summary>
        /// در استفاده از این متد دقت کنید. این متد فقط در صورتی که کاربر جاری اجازه کامل داشته باشد و یا بصورت انحصاری خود کاربر ایجاد کننده آن باشد
        /// استفاده صحیحی دارد.
        /// در صورتی که کاربر جاری اجازه خود را از دسترسی دیگری(مثلا از اجازه قلمرو) دریافت کند این متد مقدار "فالس" بر می گرداند
        /// که صحیح نیست. پس لازم است متد کاملتری نوشته شود
        /// </summary>
        /// <param name="permissionObject"></param>
        /// <param name="requestedPermissions"></param>
        /// <param name="itemCreatorUserName"></param>
        /// <returns></returns>
        public bool? HasPermission(P permissionObject, long requestedPermissions/*NOTE:this parameter can be cumulative*/, string itemCreatorUserName = null)
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated) return false;
            PermissionLimiterBase<P, PC> permissionLimiter = null;

            if (itemCreatorUserName == null) return HasPermission(permissionObject, requestedPermissions, permissionLimiter);

            permissionLimiter =
                new PL()
                .ForPermissions(PermissionCoordinator.GetAllOwnRelatedPermissions(permissionObject))
                .MakeLimitsBy(() => this.UserName == itemCreatorUserName);


            return HasPermission(permissionObject, requestedPermissions, permissionLimiter);
        }

        public override bool? HasPermission(long permissionObject, long requestedPermissions, PermissionLimiterBase permissionLimiter = null,object categoryKey=null)
        {
            return HasPermission((P)Enum.Parse(typeof(P), permissionObject.ToString()), requestedPermissions, permissionLimiter);
        }

        public bool? HasPermission(P permissionObject, long requestedPermissions/*NOTE:this parameter can be cumulative*/, PermissionLimiterBase permissionLimiter)
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated) return false;
            return UserPermissionHelper.HasPermission(permissionObject, requestedPermissions, permissionLimiter);
        }

        /// <summary>
        /// از ارسال اجازه های ترکیبی به این پارامتر خودداری شود
        /// </summary>
        /// <param name="permissionObject"></param>
        /// <param name="requestedPermissions"></param>
        /// <returns></returns>
        public bool HasFullPermissionOf(P permissionObject, long requestedPermissions)
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated) return false;
            return UserPermissionHelper.HasFullPermissionOf(permissionObject, requestedPermissions);
        }

        private UserPermissionHelper<P, PC> UserPermissionHelper
        {
            get
            {
                UserPermissionHelper<P, PC> uph = HttpContext.Current.Session["userPermissionHelper"] as UserPermissionHelper<P, PC>;
                if (uph == null)
                {
                    uph = new UserPermissionHelper<P, PC>();
                    uph.PermissionCoordinator = PermissionCoordinator;
                    uph.UserAclRetreiver = GetUserACL;
                    uph.IsEnterpriseAdmin = IsEnterpriseAdmin;
                    HttpContext.Current.Session["userPermissionHelper"] = uph;
                }
                return uph;
            }

        }

        #endregion

        private Dictionary<P, long> GetUserACL()//Access Control List
        {
            if (CheckIfPermissionsExpired() || HttpContext.Current.Session["userPermissions"] == null)
            {
                HttpContext.Current.Session["userPermissions"] = LoadUserACL();
            }
            return HttpContext.Current.Session["userPermissions"] as Dictionary<P, long>;
        }

        protected abstract Dictionary<P, long> LoadUserACL();

        private PC _permissionCoordinator = null;
        private PC PermissionCoordinator
        {
            get
            {
                if (_permissionCoordinator == null)
                    _permissionCoordinator = LoadPermissionCoordinator();
                return _permissionCoordinator;
            }
        }

        protected abstract PC LoadPermissionCoordinator();
    
    } 

}