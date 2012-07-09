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
    public abstract class CurrentUserBase
    {
        public abstract bool? HasPermission(long permissionObject, long requestedPermissions, PermissionLimiterBase limiterFunctionChecker = null);



        protected static CurrentUserBase _instance = null;
        public delegate CurrentUserBase SingleInstanceCreatorDelegate();
        public static event SingleInstanceCreatorDelegate SingleInstanceCreator;

        public static CurrentUserBase GetActiveInstance()
        {
            if (_instance == null)
            {
                if (SingleInstanceCreator != null)
                    _instance = SingleInstanceCreator();
                else
                    throw new Exception("You should handle CurrentUserBase's SingleInstanceCreator event(static event)");

            }
            return _instance;
        }

        /// <summary>
        /// NOTE : the current version clears the WHOLE SESSION...
        /// </summary>
        public void LogOff(bool expireBorowserCookie = true)
        {
            //TODO : clearing session is not correct, it should only clear related items or flag them as invalid items!
            if (expireBorowserCookie) FormsAuthentication.SignOut();
            HttpContext.Current.Session.Clear();
            HttpContext.Current.Session.Abandon();
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="P">PermissionEntity enum</typeparam>
    /// <typeparam name="C">PermissionCoordinator</typeparam>
    /// <typeparam name="PL">PermissionLimiter</typeparam>
    public abstract class CurrentUserBase<P, C, PL> : CurrentUserBase
        where C : PermissionCoordinatorBase<P>
        where PL : PermissionLimiterBase<P, C>, new()
    {


        protected abstract string EnterpriseAdminUsername { get; }

        public MembershipUser LoginInfo
        {
            get
            {
                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    if (HttpContext.Current.Session["userInfo"] == null)
                    {
                        var user= Membership.GetUser();
                        if (user == null)
                            throw new SystemAccessException();
                        HttpContext.Current.Session["userInfo"] = user;
                    }

                    return (HttpContext.Current.Session["userInfo"] as MembershipUser);
                }
                throw new Exception("User is not authenticated yet!");
            }

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
            PermissionLimiterBase<P, C> permissionLimiter = null;

            if (itemCreatorUserName == null) return HasPermission(permissionObject, requestedPermissions, permissionLimiter);

            permissionLimiter =
                new PL()
                .ForPermissions(PermissionCoordinator.GetAllOwnRelatedPermissions(permissionObject))
                .MakeLimitsBy(() => this.UserName == itemCreatorUserName);


            return HasPermission(permissionObject, requestedPermissions, permissionLimiter);
        }

        public override bool? HasPermission(long permissionObject, long requestedPermissions, PermissionLimiterBase permissionLimiter = null)
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

        private UserPermissionHelper<P, C> UserPermissionHelper
        {
            get
            {
                UserPermissionHelper<P, C> uph = HttpContext.Current.Session["userPermissionHelper"] as UserPermissionHelper<P, C>;
                if (uph == null)
                {
                    uph = new UserPermissionHelper<P, C>();
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

        private C _permissionCoordinator = null;
        private C PermissionCoordinator
        {
            get
            {
                if (_permissionCoordinator == null)
                    _permissionCoordinator = LoadPermissionCoordinator();
                return _permissionCoordinator;
            }
        }

        protected abstract C LoadPermissionCoordinator();


        private bool CheckIfPermissionsExpired()
        {
            var expiredUsers = HttpContext.Current.Application["expiredPermissions"] as HashSet<string>;
            string username = this.UserName;
            if (expiredUsers == null || !expiredUsers.Contains(username)) return false;

            lock (expiredUsers)
            {
                expiredUsers.Remove(username);
                if (expiredUsers.Count == 0) HttpContext.Current.Application["expiredPermissions"] = null;
            }
            return true;

        }

        public void QueueUsersToExpirePermissionList(IEnumerable<string> users)
        {
            var expiredUsers = HttpContext.Current.Application["expiredPermissions"] as HashSet<string>;
            if (expiredUsers == null)
            {
                HttpContext.Current.Application["expiredPermissions"] = new HashSet<string>();
                expiredUsers = HttpContext.Current.Application["expiredPermissions"] as HashSet<string>;
            }

            lock (expiredUsers)
            {
                foreach (var user in users)
                    expiredUsers.Add(user);
            }

        }



        public ProfileCommonBase ProfileInfo
        {
            get
            {
                if (HttpContext.Current.User.Identity.IsAuthenticated)
                    return (HttpContext.Current.Profile as ProfileCommonBase);

                throw new Exception("User is not authenticated yet!");
            }

        }

        public Guid Id
        {
            get
            {
                return (Guid)LoginInfo.ProviderUserKey;
            }
        }

        public string UserName
        {
            get
            {
                return LoginInfo.UserName;
            }
        }

        public bool IsEnterpriseAdmin()
        {
            return HttpContext.Current.User.Identity.IsAuthenticated && string.Compare(EnterpriseAdminUsername, UserName, true) == 0;
        }

        public bool IsAuthenticated
        {
            get
            {
                return HttpContext.Current.User.Identity.IsAuthenticated;
            }
        }



        public override string ToString()
        {
            return ProfileInfo.ToString();
        }

        public bool IsTabularLayout
        {
            get
            {
                return !HttpContext.Current.Request.Cookies.AllKeys.Contains("isTabularLayout") ? false : bool.Parse(HttpContext.Current.Request.Cookies["isTabularLayout"].Value.ToString());
            }
            set
            {
                HttpContext.Current.Response.Cookies.Set(new HttpCookie("isTabularLayout") { Value = value.ToString(), Expires = DateTime.MaxValue });
            }
        }

        public string Theme
        {
            get
            {
                return !HttpContext.Current.Request.Cookies.AllKeys.Contains("theme") ? "Office2007" : HttpContext.Current.Request.Cookies["theme"].Value.ToString();
            }
            set
            {
                HttpContext.Current.Response.Cookies.Set(new HttpCookie("theme") { Value = value, Expires = DateTime.MaxValue });
            }
        }

        public bool IsOpenWindowOk
        {
            get
            {
                return !HttpContext.Current.Request.Cookies.AllKeys.Contains("isOpenWindowOk") ? true : bool.Parse(HttpContext.Current.Request.Cookies["isOpenWindowOk"].Value.ToString());
            }
            set
            {
                HttpContext.Current.Response.Cookies.Set(new HttpCookie("isOpenWindowOk") { Value = value.ToString(), Expires = DateTime.MaxValue });
            }
        }

        public bool IsTopPaneExapnded
        {
            get
            {
                return !HttpContext.Current.Request.Cookies.AllKeys.Contains("isOpenWindowOk") ? true : bool.Parse(HttpContext.Current.Request.Cookies["isOpenWindowOk"].Value.ToString());
            }
            set
            {
                HttpContext.Current.Response.Cookies.Set(new HttpCookie("isOpenWindowOk") { Value = value.ToString(), Expires = DateTime.MaxValue });
            }
        }

        public string BodyFontSize
        {
            get
            {
                return !HttpContext.Current.Request.Cookies.AllKeys.Contains("Body-Font-Size") ? "8pt" : HttpContext.Current.Request.Cookies["Body-Font-Size"].Value.ToString();
            }
            set
            {
                HttpContext.Current.Response.Cookies.Set(new HttpCookie("Body-Font-Size") { Value = value.ToString(), Expires = DateTime.MaxValue });
            }
        }

        public string BodyFontName
        {
            get
            {
                return !HttpContext.Current.Request.Cookies.AllKeys.Contains("Body-Font-Name") ? "Tahoma" : HttpContext.Current.Request.Cookies["Body-Font-Name"].Value.ToString();
            }
            set
            {
                HttpContext.Current.Response.Cookies.Set(new HttpCookie("Body-Font-Name") { Value = value.ToString(), Expires = DateTime.MaxValue });
            }
        }



    }

}