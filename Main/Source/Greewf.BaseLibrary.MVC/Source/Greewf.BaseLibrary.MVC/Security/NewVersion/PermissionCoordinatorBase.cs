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
    /// /// <typeparam name="C">PermissionCategory enum</typeparam>
    public abstract class PermissionCoordinatorBase<P, C>
        where C : struct
        where P : struct
    {
        protected class RelatedPermission
        {
            /// <summary>
            /// اجازه کامل 
            /// </summary>
            public long ContainerPermission = -1;
            public long SubPermission = -1;
            public P PermissionEntity;

            public RelatedPermission(P permissionEntity, long subPermission, long containerPermission)
            {
                PermissionEntity = permissionEntity;
                SubPermission = subPermission;
                ContainerPermission = containerPermission;
            }

        }

        //dont change the names. it is used in code-generation tools
        private List<RelatedPermission> lstRelatedPermissions = new List<RelatedPermission>();
        private Dictionary<Type, P> dicPermissionEntityEnumMaps = new Dictionary<Type, P>();
        private static Dictionary<P, long> dicOwnRelatedPermissions = new Dictionary<P, long>();
        private static Dictionary<P, C?> dicPermissionCategories = new Dictionary<P, C?>();

        protected PermissionCoordinatorBase()
        {
            LoadPermissionRelationships(dicPermissionEntityEnumMaps, lstRelatedPermissions, dicOwnRelatedPermissions);
            LoadPermissionCatergories(dicPermissionCategories);
        }

        private static PermissionCoordinatorBase<P, C> _instance = null;
        public static T GetInstance<T>() where T : PermissionCoordinatorBase<P, C>, new()
        {
            if (_instance == null)
                _instance = new T();

            var result = _instance as T;
            if (result == null)
                throw new Exception(string.Format("This method can be called only with the type that was passed to it first call. It is associated with '{0}' previously." + typeof(T).FullName));

            return result;
        }

        protected abstract void LoadPermissionRelationships(Dictionary<Type, P> enumMaps, List<RelatedPermission> relatedPermissions, Dictionary<P, long> ownPermissionsList);
        protected abstract void LoadPermissionCatergories(Dictionary<P, C?> categoryPermissions);

        public P GetRelatedPermissionItem(Type type)
        {
            try
            {
                return dicPermissionEntityEnumMaps[type];
            }
            catch (KeyNotFoundException x)
            {

                throw new Exception(string.Format("The given type({0}) is not mapped in PermissionCoordinator's 'LoadPermissionRelationships' method(for 'enumMaps' parameter).", type.ToString()));
            }
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

        /// <summary>
        /// آیا فقط اجازه جزیی را دارد و اجازه کامل آنرا ندارد؟
        /// </summary>
        /// <param name="permissionEntity"></param>
        /// <param name="requestedPermission"></param>
        /// <param name="userPermissions"></param>
        /// <returns></returns>
        public bool HasOnlySubPermission(P permissionEntity, long requestedPermission, long userPermissions)
        {
            //اجازه جزیی : ViewOwn
            //اجازه کامل : ViewFull

            // اگر کاربر فقط اجازه جزیی را دارد ولی کلی را ندارد و درخواست اجازه هم برای فقط جزیی بود آنگاه باید کاربر چک شود
            foreach (var item in lstRelatedPermissions.Where(o => o.PermissionEntity.Equals(permissionEntity)))
                if ((requestedPermission & item.SubPermission) != 0)//اگر اجازه درخواستی حاوی اجازه ی جزیی ای است
                    if ((userPermissions & (item.SubPermission | item.ContainerPermission)) == item.SubPermission)//اگر اجازه های کاربر فقط اجازه جزیی را دارد و اجازه کامل آنرا شامل نمی شود
                        return true;

            return false;

        }

        public C? GetPermissionCategory(P permission)
        {
            return dicPermissionCategories[permission];
        }

        public P[] GetCategoryPermissionObjects(C? category)
        {
            if (category == null)
                return dicPermissionCategories.Where(o => o.Value == null).Select(o => o.Key).ToArray();
            else
                return dicPermissionCategories.Where(o => o.Value != null && o.Value.Value.Equals(category.Value)).Select(o => o.Key).ToArray();
        }

        public List<SystemRole<P, C, R>> ParseSystemRolesText<R>(string rolesText) where R : struct
        {
            rolesText = rolesText.Replace("\r", "").Replace("\n", "");
            var systemRoles = new List<SystemRole<P, C, R>>();
            string assemblyQualifiedName = typeof(P).AssemblyQualifiedName;

            foreach (string roleText in rolesText.Split(';'))
            {
                var parts = roleText.Split('#');
                var role = new SystemRole<P, C, R>();

                if (parts[0].Trim().Length != 0)
                    role.Category = (C)Enum.Parse(typeof(C), parts[0].Trim(), true);

                role.Id = (R)Enum.Parse(typeof(R), parts[1].Trim(), true);
                role.Name = parts[2].Trim();
                if (parts.Length >= 4)
                {
                    role.EditableUsers = parts[3].Trim().ToLower() == "true" ? true : false;
                    if (parts.Length >= 5)
                    {
                        var permissions = parts[4].Trim().TrimStart('{').TrimEnd('}').Split(',');
                        foreach (var p in permissions)
                        {
                            var x = p.Split('.');
                            if (x[0].Trim().Length == 0) continue;

                            var pit = Type.GetType(assemblyQualifiedName.Replace(typeof(P).Name, x[0].Trim()), true, true);
                            var pe = this.GetRelatedPermissionItem(pit);

                            var pi = (long)Enum.Parse(pit, x[1].Trim(), true);

                            role.DefaultPermissions.Add(new KeyValuePair<P, long>(pe, pi));

                        }
                    }
                }
                systemRoles.Add(role);
            }

            return systemRoles;

        }

    }


}
