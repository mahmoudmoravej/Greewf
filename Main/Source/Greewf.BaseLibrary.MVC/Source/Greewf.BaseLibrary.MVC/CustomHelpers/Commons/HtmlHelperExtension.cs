using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System;
using System.Text;

namespace Greewf.BaseLibrary.MVC.CustomHelpers
{
    public static class HtmlHelperExtension
    {
        public static GreewfHelperFactory<TModel> Greewf<TModel>(this HtmlHelper<TModel> helper)
        {
            return new GreewfHelperFactory<TModel>(helper);
        }

        public static GreewfHelperFactory Greewf(this HtmlHelper helper)
        {
            return new GreewfHelperFactory(helper);
        }
    }

  

}
