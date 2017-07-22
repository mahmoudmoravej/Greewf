using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System;
using System.Text;

namespace Greewf.BaseLibrary.MVC.CustomHelpers
{
    
    public class ContextMenuItem
    {
        public Func<string> Template { get; set; }
        public Func<string> ClientTemplate { get; set; }
        public bool Visible { get; set; }

        public ContextMenuItem()
        {
            Visible = true;
        }
    }

    
}
