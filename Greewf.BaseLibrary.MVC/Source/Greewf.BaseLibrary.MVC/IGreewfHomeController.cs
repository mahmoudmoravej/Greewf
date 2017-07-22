using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Greewf.BaseLibrary.MVC
{
    public interface IGreewfHomeController
    {
        ActionResult SavedSuccessfully();
        ActionResult AccessDenied();
        ActionResult Error();
    }
}
