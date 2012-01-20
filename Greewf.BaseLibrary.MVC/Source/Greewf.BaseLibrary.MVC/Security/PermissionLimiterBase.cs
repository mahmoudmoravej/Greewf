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

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="P">PermissionEntity</typeparam>
    public abstract class PermissionLimiterBase<P, C> : PermissionLimiterBase
        where C : PermissionCoordinatorBase<P>
    {

        protected abstract C PermissionCoordinator
        {
            get;
        }


        public PermissionLimiterBase<P, C> ForPermissions<Y>(Y permissions) where Y : struct
        {
            LimiterPermission = (long)Convert.ChangeType(permissions, typeof(Y));
            return this;
        }

        public PermissionLimiterBase<P, C> ForOwnsPermissionsOf(P permissionEntity)
        {
            LimiterPermission = PermissionCoordinator.GetAllOwnRelatedPermissions(permissionEntity);
            return this;
        }


        public PermissionLimiterBase<P, C> MakeLimitsBy(Func<bool> limitterFunction)
        {
            LimiterFunction = limitterFunction;
            return this;
        }

        public PermissionLimiterBase<P, C> OrPart()
        {
            IsAndPart = false;
            return this;
        }


        public PermissionLimiterBase<P, C> AndPart()
        {
            IsAndPart = true;
            return this;
        }

        public PermissionLimiterBase<P, C> Message(Func<string> errorMessage)
        {
            this.ErrorMessage = errorMessage;
            return this;
        }
    }
}
