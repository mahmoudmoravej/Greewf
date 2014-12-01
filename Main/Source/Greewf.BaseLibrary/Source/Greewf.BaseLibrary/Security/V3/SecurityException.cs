using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Authentication;


namespace Greewf.BaseLibrary.Security.V3
{
    public class SecurityException : AuthenticationException
    {
        public string[] ErrorMessages { get; set; }
        public int PermissionGroup { get; private set; }
        public long Permissions { get; private set; }
        public int UserId { get; private set; }
        public SecurityException(int group, long permissions, int userId)
        {
            this.PermissionGroup = group;
            this.Permissions = permissions;
            this.UserId = userId;
        }

        public SecurityException(int group, long permissions, string[] messages)
        {
            this.PermissionGroup = group;
            this.Permissions = permissions;
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