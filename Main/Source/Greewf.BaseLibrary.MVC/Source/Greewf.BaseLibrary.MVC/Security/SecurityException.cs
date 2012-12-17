using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Authentication;


namespace Greewf.BaseLibrary.MVC.Security
{
    public class SecurityException : AuthenticationException
    {
        public string[] ErrorMessages { get; set; }

        public SecurityException( long permissionObject, string username = "")
        {

        }

        public SecurityException( long permissionObject, string[] messages, string username = "")
        {
            ErrorMessages = messages;
        }

        public SecurityException(string message)
        {
            ErrorMessages = new string[] { message };
        }
    }

    public class SystemAccessException : SecurityException
    {

        public SystemAccessException()
            : base("Have No Access To The Current System")
        {

        }

        public SystemAccessException(string msg)
            : base(msg)
        {
        }
    }
}