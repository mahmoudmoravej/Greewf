using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Greewf.BaseLibrary.MVC.Security
{

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="P">PermissionEntity</typeparam>
    public abstract class PermissionLimiterBase<P, PC> : PermissionLimiterBase
        where P : struct
        where PC : PermissionCoordinatorBase<P>
    {

        protected abstract PC PermissionCoordinator
        {
            get;
        }


        public PermissionLimiterBase<P, PC> ForPermissions<Y>(Y permissions) where Y : struct
        {
            LimiterPermission = (long)Convert.ChangeType(permissions, typeof(Y));
            return this;
        }

        public PermissionLimiterBase<P, PC> ForOwnsPermissionsOf(P permissionEntity)
        {
            LimiterPermission = PermissionCoordinator.GetAllOwnRelatedPermissions(permissionEntity);
            return this;
        }

     


        public PermissionLimiterBase<P, PC> MakeLimitsBy(Func<bool> limitterFunction)
        {
            LimiterFunction = limitterFunction;
            return this;
        }

        public PermissionLimiterBase<P, PC> OrPart()
        {
            IsAndPart = false;
            return this;
        }


        public PermissionLimiterBase<P, PC> AndPart()
        {
            IsAndPart = true;
            return this;
        }

        public PermissionLimiterBase<P, PC> Message(Func<string> errorMessage)
        {
            this.ErrorMessage = errorMessage;
            return this;
        }
    }

}
