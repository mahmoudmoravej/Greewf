using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Greewf.BaseLibrary.Logging.LogContext;
using System.Web;

namespace Greewf.BaseLibrary.Logging
{

    public class DefaultLogger : Logger
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
