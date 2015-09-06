using Greewf.BaseLibrary.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Greewf.BaseLibrary.MVC.Logging
{
    /// <summary>
    /// This class is HttpContext.Current oriented. In other words it is good for Mvc & web form projects.
    /// NOTE : it cannot be used with WebApi projects
    /// </summary>
    public abstract class HttpWebLogger : LoggerBase
    {
        private static HttpWebLogger current = new DefaultLogger();
        public static HttpWebLogger Current
        {
            get
            {
                return current;
            }
            set
            {
                if (value == null)
                    current = new DefaultLogger();
                else
                    current = value;
            }
        }

        protected override void ReadRequestToLog(ref Log log, object extraData)
        {
            var request = HttpContext.Current.Request;

            log.Browser = request.Browser.Browser + request.Browser.Version;
            log.IsMobile = request.Browser.IsMobileDevice;
            log.UserAgent = request.UserAgent;
            log.Ip = request.UserHostAddress;
            log.MachineName = request.UserHostName;
            log.RequestUrl = request.Url.GetLeftPart(UriPartial.Path);
            log.Querystring = request.QueryString.ToString();
            log.RequestBody = request.Form == null ? null : request.Form.ToString();
            log.RequestMethod = request.HttpMethod;
            log.RequestHeaders = request.Headers == null ? null : request.Headers.ToString();
        }

        protected override string ReadUsername()
        {
            string value = null;
            try//becuase of some reasons(like too early exception in HttpApplication) we may get exception on calling Username & UserFullName properties
            {
                value = Username;
            }
            catch (Exception x)
            {
                if (HttpContext.Current.User != null && HttpContext.Current.User.Identity != null && HttpContext.Current.User.Identity.IsAuthenticated)
                    value = HttpContext.Current.User.Identity.Name;
                else
                    value = x.ToString();
            }

            return value;
        }

    }

}
