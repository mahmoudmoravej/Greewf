using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Greewf.BaseLibrary.Logging;
using System.Web;

namespace Greewf.BaseLibrary.MVC.Logging
{

    public class DefaultLogger : HttpWebLogger
    {
        public override string Username
        {
            get
            {
                if (HttpContext.Current.User != null && HttpContext.Current.User.Identity != null && HttpContext.Current.User.Identity.IsAuthenticated)
                    return HttpContext.Current.User.Identity.Name;
                return "";
            }
        }

        public override string UserFullName
        {
            get
            {
                return "";
            }
        }
    }


}
