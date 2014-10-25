using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Authentication;


namespace Greewf.BaseLibrary.Security.V3
{
    public class SecurityException : AuthenticationException
    {
        public string[] ErrorMessages { get; set; }

        public SecurityException(int group, long permissions, int userId)
        {

        }

        public SecurityException(int group, long permissions, string[] messages)
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