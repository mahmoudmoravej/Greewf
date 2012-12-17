using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Greewf.BaseLibrary.MVC.Security
{

    public abstract class PermissionLimiterBase<P, C, PC> : PermissionLimiterBase
        where P : struct
        where C : struct
        where PC : PermissionCoordinatorBase<P, C>
    {

        protected abstract PC PermissionCoordinator
        {
            get;
        }


        public PermissionLimiterBase<P, C, PC> On<Y>(Y permissions) where Y : struct
        {
            LimiterPermission = (long)Convert.ChangeType(permissions, typeof(Y));
            return this;
        }

        //public PermissionLimiterBase<P, C, PC> ForPermissions<Y>(Y permissions) where Y : struct
        //{
        //    LimiterPermission = (long)Convert.ChangeType(permissions, typeof(Y));
        //    return this;
        //}

        public PermissionLimiterBase<P, C, PC> OnOwnsPermissionsOf(P permissionEntity)
        {
            LimiterPermission = PermissionCoordinator.GetAllOwnRelatedPermissions(permissionEntity);
            return this;
        }

        //public PermissionLimiterBase<P, C, PC> ForOwnsPermissionsOf(P permissionEntity)
        //{
        //    LimiterPermission = PermissionCoordinator.GetAllOwnRelatedPermissions(permissionEntity);
        //    return this;
        //}
       

        //public PermissionLimiterBase<P, C, PC> MakeLimitsBy(Func<bool> limitterFunction)
        //{
        //    LimiterFunction = limitterFunction;
        //    return this;
        //}

        public PermissionLimiterBase<P, C, PC> Except(Func<bool> limitterFunction)
        {
            LimiterFunction = limitterFunction;
            return this;
        }

        public PermissionLimiterBase<P, C, PC> OrPart()
        {
            IsAndPart = false;
            return this;
        }


        public PermissionLimiterBase<P, C, PC> AndPart()
        {
            IsAndPart = true;
            return this;
        }

        public PermissionLimiterBase<P, C, PC> Message(Func<string> errorMessage)
        {
            this.ErrorMessage = errorMessage;
            return this;
        }
    }
}
