using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Greewf.BaseLibrary.MVC.Security
{
    /// <summary>
    /// توجه : این کلاس برای اعمال محدودیت تنها بر روی اجازه هایی هست که فول نیستند و محدود کننده هستند
    /// لذا برای اجازه های فول اصلا نمی بایست از این کلاس استفاده کرد
    /// </summary>
    public abstract class PermissionLimiterBase
    {
        internal protected long? LimiterPermission { get; protected set; }
        internal protected Func<bool> LimiterFunction { get; protected set; }
        internal protected bool IsAndPart { get; protected set; }
        internal protected Func<string> ErrorMessage { get; protected set; }
    }

}
