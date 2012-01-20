using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Authentication;


namespace Greewf.BaseLibrary.MVC.Security
{
    public class SecurityException : AuthenticationException
    {
        public string[] ErrorMessages { get; set; }

        public SecurityException(long permissionOBject, string username = "")
        {

        }

        public SecurityException(long permissionOBject,  string[] messages, string username = "")
        {
            ErrorMessages = messages;
        }

    }
}