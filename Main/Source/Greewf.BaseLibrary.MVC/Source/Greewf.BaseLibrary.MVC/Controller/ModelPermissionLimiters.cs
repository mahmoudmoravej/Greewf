using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Greewf.BaseLibrary.MVC.Security;
using System.Web.Routing;
using System.Text;
using System.Data.Entity;
using Greewf.BaseLibrary.Repositories;
using Greewf.BaseLibrary.MVC.Logging;
using Greewf.BaseLibrary.MVC.Ajax;
using System.Linq.Expressions;

namespace Greewf.BaseLibrary.MVC
{  

    public class ModelPermissionLimiters
    {
        public PermissionLimiterBase[] LimiterFunctions { get; set; }
    }

 

}