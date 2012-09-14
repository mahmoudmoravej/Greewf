using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Greewf.BaseLibrary.MVC.Logging.LogContext;
using System.Web;

namespace Greewf.BaseLibrary.MVC.ChangeTracker
{

    public class DefaultChangeTracker : ChangeTracker
    {
        public override string UserId
        {
            get
            {
                if (HttpContext.Current.User != null && HttpContext.Current.User.Identity != null && HttpContext.Current.User.Identity.IsAuthenticated)
                    return HttpContext.Current.User.Identity.Name;
                return "";
            }
        }
       
    }


}
