using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Greewf.BaseLibrary.MVC.Security
{

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="P">Permission Entity Enum</typeparam>
    public abstract class PermissionCoordinatorBase<P>
    {
        protected class RelatedPermission
        {
            /// <summary>
            /// اجازه ای که اجازه محدود شده را نیز شامل می شود
            /// </summary>
            public long ContainerPermission = -1;
            public long LimitedPermission = -1;
            public P PermissionEntity;

            public RelatedPermission(P permissionEntity, long limitedPermission, long containerPermission)
            {
                PermissionEntity = permissionEntity;
                LimitedPermission = limitedPermission;
                ContainerPermission = containerPermission;
            }

        }

        //dont change the names. it is used in code-generation tools
        private List<RelatedPermission> lstRelatedPermissions = new List<RelatedPermission>();
        private Dictionary<Type, P> dicPermissionEntityEnumMaps = new Dictionary<Type, P>();
        private static Dictionary<P, long> dicOwnRelatedPermissions = new Dictionary<P, long>();

        protected PermissionCoordinatorBase()
        {
            LoadPermissionRelationships(dicPermissionEntityEnumMaps, lstRelatedPermissions, dicOwnRelatedPermissions);
        }

        private static PermissionCoordinatorBase<P> _instance = null;
        public static T GetInstance<T>() where T : PermissionCoordinatorBase<P>, new()
        {
            if (_instance == null)
                _instance = new T();

            var result = _instance as T;
            if (result == null)
                throw new Exception(string.Format("This method can be called only with the type that was passed to it first call. It is associated with '{0}' previously." + typeof(T).FullName));

            return result;
        }

        protected abstract void LoadPermissionRelationships(Dictionary<Type, P> enumMaps, List<RelatedPermission> relatedPermissions, Dictionary<P, long> ownPermissionsList);

        public P GetRelatedPermissionItem(Type type)
        {
            return dicPermissionEntityEnumMaps[type];
        }

        public long GetAllOwnRelatedPermissions(P permissionEntity)
        {
            try
            {
                return dicOwnRelatedPermissions[permissionEntity];
            }
            catch (Exception x)
            {
                throw new Exception("Dear Developer: The passed permissionEntity is not defined in dicOwnRelatedPermissions dictionary in PermissionCoordinator", x);
            }
        }

        public bool IsLimitedPermission(P permissionEntity, long requestedPermission, long userPermissions)
        {

            // اگر کاربر فقط اجازه محدود شده را دارد ولی کلی را ندارد و درخواست اجازه هم برای فقط محدود شده بود آنگاه باید کاربر چک شود
            foreach (var item in lstRelatedPermissions.Where(o => o.PermissionEntity.Equals(permissionEntity)))
                if ((requestedPermission & item.LimitedPermission) != 0)//اگر اجازه درخواستی حاوی اجازه ی محدود شده ای است
                    if ((userPermissions & (item.LimitedPermission | item.ContainerPermission)) == item.LimitedPermission)//اگر اجازه های کاربر فقط اجازه محدود شده را دارد و اجازه کلی آنرا شامل نمی شود
                        return true;

            return false;

        }

    }

  

}
