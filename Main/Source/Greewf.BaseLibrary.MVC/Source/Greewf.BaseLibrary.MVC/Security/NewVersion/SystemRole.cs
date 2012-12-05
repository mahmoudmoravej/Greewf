using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Greewf.BaseLibrary.MVC.Security
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="C">PermissionCategory</typeparam>
    /// <typeparam name="P">PermissionEntity(PermissionObject)</typeparam>
    /// <typeparam name="R">System Role Enum (Map to Id)</typeparam>
    public class SystemRole<P, C, R>
        where P : struct
        where C : struct
        where R : struct
    {
        public SystemRole()
        {
            DefaultPermissions = new List<KeyValuePair<P, long>>();
        }

        public string Name { get; set; }

        public C? Category { get; set; }

        public R Id { get; set; }

        public List<KeyValuePair<P, long>> DefaultPermissions { get; private set; }

        public bool EditableUsers { get; set; }

        public bool Visible { get; set; }

    }
}