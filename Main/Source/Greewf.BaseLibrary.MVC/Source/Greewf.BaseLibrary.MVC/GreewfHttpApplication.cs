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
        private readonly bool _doCustomError = bool.Parse(System.Configuration.ConfigurationManager.AppSettings["CustomError"] ?? "false");
        private readonly CustomErrorDetailsMode _customErrorDetailsMode = (CustomErrorDetailsMode)Enum.Parse(typeof(CustomErrorDetailsMode), System.Configuration.ConfigurationManager.AppSettings["CustomErrorDetailsMode"] ?? "None");

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
            //NOTE 1! : CustomErrors should set to be Off to get this function call 
            //NOTE 2! : If any error occured during this function execution, the first exception returns to the browser.

            var error = Server.GetLastError();

            if (error is SecurityException)
                HandleSecurityExceptions(error);
            else//regular error
            {
                long logId = 0;
                try
                {
                    logId = Logger.Current.Log(RegularExceptionLogPointId, error);
                }
                catch { }

                HandleCustomErrors(error, logId);
            }

        }

        private void HandleSecurityExceptions(Exception error)
        {
            Logger.Current.Log(SecurityExceptionLogPointId, (error as SecurityException).ErrorMessages);
            string querystring = PrepareQuerystring(error);

            querystring = querystring.Trim('&');

            Server.ClearError();
            Session["ErrorMessages"] = (error as SecurityException).ErrorMessages;
            Response.Redirect("~/home/accessdenied" + (querystring.Length > 0 ? "?" + querystring : ""));
        }

        private void HandleCustomErrors(Exception error, long logId)
        {
            if (_doCustomError)
                if (!Request.Url.AbsolutePath.EndsWith("/Home/Error"))//the error is not raised in the error handler page
                {
                    string querystring = PrepareQuerystring(error);

                    Server.ClearError();
                    CompleteErrorMessageSession(error, logId);

                    Response.Redirect("~/Home/Error" + (querystring.Length > 0 ? "?" + querystring : ""));

                }
                else//the error raised in error handler page (so we should ignore redirection)
                {

                    Response.Write("<h2>خطای ناشناخته در صفحه نمایش خطا</h2>\n");
                    Response.Write("<p>" + GetErrorMessage(error) + "</p>\n");
                    if (logId > 0)
                        Response.Write("<p>شماره رخداد ثبت شده : " + logId + "</p>\n");
                    else if (logId == -1)
                        Response.Write("<p>شماره رخداد ثبت شده : غیر فعال </p>\n");
                    else if (logId == 0)//exception occured throw logging
                        Response.Write("<p>شماره رخداد ثبت شده : خطا در ثبت رخداد </p>\n");

                    Server.ClearError();
                    Response.StatusCode = 500;//to make ajax call enable getting it through onError event
                    Response.Headers.Add("GreewfCustomErrorPage", "true"); //to help ajax onError event to distinguish between regular content or custom error page content.

                }
        }

        private void CompleteErrorMessageSession(Exception error, long logId)
        {
            var errorMessages = new string[] { logId.ToString(), "" };
            errorMessages[1]= GetErrorMessage(error);

            Session["ErrorMessages"] = errorMessages;
        }

        private string GetErrorMessage(Exception error)
        {
            switch (_customErrorDetailsMode)
            {
                case CustomErrorDetailsMode.None:
                    return null;
                case CustomErrorDetailsMode.Header:
                    return error.Message;
                case CustomErrorDetailsMode.Complete:
                    return error.ToString();
                case CustomErrorDetailsMode.LocalComplete:
                    if (Request.IsLocal)
                        return error.ToString();
                    else
                        return null;
                default:
                    return error.ToString();
            }
        }

        private string PrepareQuerystring(Exception error)
        {
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
            return querystring;
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

        private enum CustomErrorDetailsMode
        {
            None,
            Header,
            Complete,
            LocalComplete,//None on remote
        }

    }

}
