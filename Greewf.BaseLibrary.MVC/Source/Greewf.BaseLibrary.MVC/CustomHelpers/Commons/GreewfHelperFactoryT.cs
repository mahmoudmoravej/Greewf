using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System;
using System.Text;

namespace Greewf.BaseLibrary.MVC.CustomHelpers
{

    public partial class GreewfHelperFactory<TModel> : GreewfHelperFactory
    {
        public GreewfHelperFactory(HtmlHelper<TModel> helper)
            : base(helper)
        {
        }
    }

}
