using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System;
using System.Text;

namespace Greewf.BaseLibrary.MVC.CustomHelpers
{
 

    public partial class GreewfHelperFactory
    {
        protected HtmlHelper Helper { get; private set; }

        public GreewfHelperFactory(HtmlHelper helper)
        {
            this.Helper = helper;
        }

        public IHtmlString RenderScripts()
        {
            string output = GetContextMenuStartupScript();
            return new HtmlString(output);
        }
    }

  

}
