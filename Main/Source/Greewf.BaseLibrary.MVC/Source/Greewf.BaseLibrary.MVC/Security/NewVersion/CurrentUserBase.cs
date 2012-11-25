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


    public abstract class CurrentUserBase<P, C, PC, PL, K> : CurrentUserBase
        where P : struct
        where C : struct
        where K : struct
        where PC : PermissionCoordinatorBase<P, C>
        where PL : PermissionLimiterBase<P, C, PC>, new()
    {





        #region Permissions

        public bool HasPermission<T>(T permissions, string itemCreatorUserName = null, K? categoryKey = null) where T : struct
        {
            var entityItem = PermissionCoordinator.GetRelatedPermissionItem(typeof(T));
            return HasPermission(entityItem, Convert.ToInt64(permissions), itemCreatorUserName, categoryKey) == true;
        }

        /// <summary>
        /// از ارسال اجازه های ترکیبی به این پارامتر خودداری شود
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public bool HasFullPermissionOf<T>(T permissions, K? categoryKey = null) where T : struct
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated) return false;
            return UserPermissionHelper.HasFullPermissionOf<T>(permissions, categoryKey);
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
        public bool? HasPermission(P permissionObject, long requestedPermissions/*NOTE:this parameter can be cumulative*/, string itemCreatorUserName = null, K? categoryKey = null)
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated) return false;
            PermissionLimiterBase<P, C, PC> permissionLimiter = null;

            if (itemCreatorUserName == null) return HasPermission(permissionObject, requestedPermissions, permissionLimiter, categoryKey);

            permissionLimiter =
                new PL()
                .ForPermissions(PermissionCoordinator.GetAllOwnRelatedPermissions(permissionObject))
                .MakeLimitsBy(() => this.UserName == itemCreatorUserName);


            return HasPermission(permissionObject, requestedPermissions, permissionLimiter);
        }

        public override bool? HasPermission(long permissionObject, long requestedPermissions, PermissionLimiterBase permissionLimiter = null, object categoryKey = null)
        {
            return HasPermission((P)Enum.Parse(typeof(P), permissionObject.ToString()), requestedPermissions, permissionLimiter, (K?)categoryKey);
        }

        public bool? HasPermission(P permissionObject, long requestedPermissions/*NOTE:this parameter can be cumulative*/, PermissionLimiterBase permissionLimiter, K? categoryKey = null)
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated) return false;
            return UserPermissionHelper.HasPermission(permissionObject, requestedPermissions, permissionLimiter, categoryKey);
        }

        /// <summary>
        /// از ارسال اجازه های ترکیبی به این پارامتر خودداری شود
        /// </summary>
        /// <param name="permissionObject"></param>
        /// <param name="requestedPermissions"></param>
        /// <returns></returns>
        public bool HasFullPermissionOf(P permissionObject, long requestedPermissions, K? categoryKey = null)
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated) return false;
            return UserPermissionHelper.HasFullPermissionOf(permissionObject, requestedPermissions, categoryKey);
        }

        /// <summary>
        /// بدون توجه به رسته اجازه ها، چک می کند که آیا اجازه ای دارد. برای زمانی مناسب است که مثلا می خواهیم یک منو رو فعال یا غیرفعال کنیم ولی رسته ان بعدا مشخص می شود
        /// مثال : آیا اجازه ارسال دستور حداقل در یک استان را دارد؟
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public bool HasAnyCategoryPermission<T>(T permissions) where T : struct
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated) return false;
            return UserPermissionHelper.HasAnyCategoryPermission(permissions);
        }

        public override bool HasAnyCategoryPermission(long permissionObject, long permissions, PermissionLimiterBase limiterFunctionChecker)
        {
            if (!HttpContext.Current.User.Identity.IsAuthenticated) return false;
            return UserPermissionHelper.HasAnyCategoryPermission((P)Enum.Parse(typeof(P), permissionObject.ToString()), permissions, limiterFunctionChecker);

        }

        public List<K?> GetAllowedCategoryObjects<T>(T permissions) where T : struct
        {
            return UserPermissionHelper.GetAllowedCategoryObjects(permissions);
        }

        private UserPermissionHelper<P, C, PC, K> UserPermissionHelper
        {
            get
            {
                UserPermissionHelper<P, C, PC, K> uph = HttpContext.Current.Session["userPermissionHelperNewModel"] as UserPermissionHelper<P, C, PC, K>;
                if (uph == null)
                {
                    uph = new UserPermissionHelper<P, C, PC, K>();
                    uph.PermissionCoordinator = PermissionCoordinator;
                    uph.UserAclRetreiver = GetUserACL;
                    uph.IsEnterpriseAdmin = IsEnterpriseAdmin;
                    HttpContext.Current.Session["userPermissionHelperNewModel"] = uph;
                }
                return uph;
            }

        }

        #endregion

        protected internal override object GetPermissionCategoryKey(long permissionObject, IEnumerable<long> permissions, object entityKey)
        {
            object obj = permissionObject;
            var cat = PermissionCoordinator.GetPermissionCategory((P)obj);
            return GetPermissionCategoryKey(cat);
        }

        /// <summary>
        /// مناسب برای حالت هایی که در هنگام ورود نوع اش را مشخص می کند
        /// </summary>
        /// <param name="permissionCategory"></param>
        /// <returns></returns>
        protected abstract K? GetPermissionCategoryKey(C? permissionCategory);

        private Dictionary<UserAclKey<C, K>, Dictionary<P, long>> GetUserACL()//Access Control List
        {
            if (CheckIfPermissionsExpired() || HttpContext.Current.Session["userPermissions"] == null)
            {
                HttpContext.Current.Session["userPermissions"] = LoadUserACL();
            }
            return HttpContext.Current.Session["userPermissions"] as Dictionary<UserAclKey<C, K>, Dictionary<P, long>>;
        }

        protected abstract Dictionary<UserAclKey<C, K>, Dictionary<P, long>> LoadUserACL();

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

        protected override abstract string EnterpriseAdminUsername { get; }







    }

}