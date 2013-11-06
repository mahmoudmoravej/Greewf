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
            string output = @"
                <script>
                    $(document).on('mouseenter', '.g-context-menu', function () {
                        var mnu=$(this); 
                        if (!mnu.data('t-menu')) {
                            mnu.parent().css('overflow', 'visible'); 
                            var tmnu= mnu.tMenu().data('t-menu') ; 
                            tmnu.openOnClick = mnu.attr('g-open-onclick'); 
                            if(!tmnu.openOnClick){
                                tmnu.open(mnu.find('>li'));
                            } 
                        } 
                    });
                </script>";
            return new HtmlString(output);
        }
    }

  

}
