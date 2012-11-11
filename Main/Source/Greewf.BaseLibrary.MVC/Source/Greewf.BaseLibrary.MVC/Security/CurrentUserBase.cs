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
        public abstract bool? HasPermission(long permissionObject, long requestedPermissions, PermissionLimiterBase limiterFunctionChecker = null, object categoryKey = null);
        public abstract bool HasAnyCategoryPermission(long permissionObject, long requestedPermissions, PermissionLimiterBase limiterFunctionChecker);

        protected internal abstract object GetPermissionCategoryKey(long permissionObject);

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
        protected abstract string EnterpriseAdminUsername { get; }       


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

        public MembershipUser LoginInfo
        {
            get
            {
                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    if (HttpContext.Current.Session["userInfo"] == null)
                    {
                        var user = Membership.GetUser();
                        if (user == null)
                            throw new SystemAccessException();
                        HttpContext.Current.Session["userInfo"] = user;
                    }

                    return (HttpContext.Current.Session["userInfo"] as MembershipUser);
                }
                throw new Exception("User is not authenticated yet!");
            }

        }

        protected bool CheckIfPermissionsExpired()
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