using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;
using System.Security.Principal;
using Greewf.BaseLibrary.MVC.Security;
using Greewf.BaseLibrary.MVC.Logging;
/// ref1 : http://msdn.microsoft.com/en-us/library/eb0zx8fc.aspx
/// ref2 : http://blog.ie-soft.de/post/2007/12/globalasax-events.aspx
/// ref3 :http://msdn.microsoft.com/en-us/library/1d3t3c61.aspx


namespace Greewf.BaseLibrary.MVC
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">LogPoint Enum</typeparam>
    public abstract class GreewfHttpApplication<T> : System.Web.HttpApplication
        where T : struct
    {
        private const string COOKIESESSIONKEY = ".ASPXAUTHLASTCOOKIE_";

        protected abstract T RegularExceptionLogPointId { get; }

        protected abstract T SecurityExceptionLogPointId { get; }

        protected virtual void Application_PostAcquireRequestState(object sender, EventArgs args)
        {
            //NOTE : we use "Application_PostAcquireRequestState" instead of "Application_PostAuthenticateRequest" becuase the session is not ready yet there.

            if (Context.Request.IsAuthenticated &&
                Context.Session != null &&
                (string)Session[COOKIESESSIONKEY] != Request.Cookies[FormsAuthentication.FormsCookieName].Value)
            {

                var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                if (authCookie == null) return;//it should not be happened but we put it for insurance

                if (Session[COOKIESESSIONKEY] != null)
                {
                    CurrentUserBase.GetActiveInstance().LogOff(false);

                    //Extract the forms authentication cookie
                    var authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                    var userPrincipal = new GenericPrincipal(new System.Web.Security.FormsIdentity(authTicket), null);
                    Context.User = userPrincipal;
                }
                Session[COOKIESESSIONKEY] = authCookie.Value;

            }
        }

        protected virtual void Application_Error()
        {

            var error = Server.GetLastError();
            if (error is SecurityException)
            {
                Logger.Current.Log(SecurityExceptionLogPointId, (error as SecurityException).ErrorMessages);
                string querystring = "";

                if (Request.QueryString.AllKeys.Contains("simplemode"))
                    querystring += "&simplemode=1";
                if (Request.QueryString.AllKeys.Contains("puremode"))
                    querystring += "&puremode=1";
                if (Request.QueryString.AllKeys.Contains("iswindow"))
                    querystring += "&iswindow=1";
                if (Request.QueryString.AllKeys.Contains("includeUrlInContent"))
                    querystring += "&includeUrlInContent=1";
                if (error is SystemAccessException)
                    querystring += "&systemAccessError=1";


                querystring = querystring.Trim('&');

                Server.ClearError();
                Session["ErrorMessages"] = (error as SecurityException).ErrorMessages;
                Response.Redirect("~/home/accessdenied" + (querystring.Length > 0 ? "?" + querystring : ""));
            }
            else
            {
                Logger.Current.Log(RegularExceptionLogPointId, error);
            }

        }

        protected virtual void Application_Start()
        {
            CheckAndDoCustomMappings();
        }


        private bool _customMappingDone = false;
        protected bool CheckAndDoCustomMappings()
        {
            if (!_customMappingDone)
            {
                _customMappingDone = true;
                DoCustomMAppings();
                return true;
            }
            return false;
        }

        protected abstract void DoCustomMAppings();



    }
}
