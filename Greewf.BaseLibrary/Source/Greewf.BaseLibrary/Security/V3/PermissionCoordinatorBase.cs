using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Greewf.BaseLibrary.Security.V3
{

    public abstract class PermissionCoordinatorBase
    {
        protected static PermissionCoordinatorBase _instance = null;
        public delegate PermissionCoordinatorBase SingleInstanceCreatorDelegate();
        public static event SingleInstanceCreatorDelegate SingleInstanceCreator;

        public static PermissionCoordinatorBase GetActiveInstance()
        {
            if (_instance == null)
            {
                if (SingleInstanceCreator != null)
                    _instance = SingleInstanceCreator();
                else
                    throw new Exception("You should handle PermissionCoordinatorBase's SingleInstanceCreator event(static event)");

            }
            return _instance;
        }

        public abstract Task<bool> HasPermissionAsync(int userId, IPermissionObject obj, int group, long permission, string permissionCondition = null);
        public abstract bool HasPermission(int userId, IPermissionObject obj, int group, long permission, string permissionCondition = null);
        public abstract bool HasAnyPermission(int userId, int group, long permission, string permissionCondition = null);
        public abstract string GetAppropriateExceptionMessage(SecurityException exception);


    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="G">Permission Group</typeparam>
    /// <typeparam name="OT">Permission Object Type</typeparam>
    public abstract class PermissionCoordinatorBase<OT, G> : PermissionCoordinatorBase
        where G : struct
        where OT : struct
    {


        private List<RelatedPermission<G>> lstRelatedPermissions;
        private Dictionary<Type, G> dicTypeGroupMaps;
        private Dictionary<G, OT> dicGroupObjectTypeMaps;
        private Dictionary<OT, OT[]> dicObjectTypeParents;

        protected PermissionCoordinatorBase()
        {
            dicGroupObjectTypeMaps = LoadGroupObjectTypeMaps() ?? new Dictionary<G, OT>();
            dicTypeGroupMaps = LoadTypeGroupMaps() ?? new Dictionary<Type, G>();
            dicObjectTypeParents = LoadObjectTypeParents() ?? new Dictionary<OT, OT[]>();
            lstRelatedPermissions = LoadRelationships() ?? new List<RelatedPermission<G>>();

        }

        //private static PermissionCoordinatorBase<G, OT> _instance = null;
        //public static T GetInstance<T>() where T : PermissionCoordinatorBase<G, OT>, new()
        //{
        //    if (_instance == null)
        //        _instance = new T();

        //    var result = _instance as T;
        //    if (result == null)
        //        throw new Exception(string.Format("This method can be called only with the type that was passed to it first call. It is associated with '{0}' previously." + typeof(T).FullName));

        //    return result;
        //}

        protected abstract List<RelatedPermission<G>> LoadRelationships();
        protected abstract Dictionary<Type, G> LoadTypeGroupMaps();
        protected abstract Dictionary<G, OT> LoadGroupObjectTypeMaps();
        protected abstract Dictionary<OT, OT[]> LoadObjectTypeParents();


        public G GetRelatedPermissionItem(Type type)
        {
            try
            {
                return dicTypeGroupMaps[type];
            }
            catch (KeyNotFoundException)
            {
                throw new Exception(string.Format("The given type({0}) is not mapped in PermissionCoordinator's 'LoadPermissionRelationships' method(for 'enumMaps' parameter).", type.ToString()));
            }
        }


        /// <summary>
        /// آیا فقط اجازه جزیی را دارد و اجازه کامل آنرا ندارد؟
        /// </summary>
        /// <param name="group"></param>
        /// <param name="requestedPermission"></param>
        /// <param name="userPermissions"></param>
        /// <returns></returns>
        public bool HasOnlyExclusive(G group, long requestedPermission, long userPermissions)
        {
            //اجازه جزیی : ViewOwn
            //اجازه کامل : ViewFull

            // اگر کاربر فقط اجازه جزیی را دارد ولی کلی را ندارد و درخواست اجازه هم برای فقط جزیی بود آنگاه باید کاربر چک شود
            foreach (var item in lstRelatedPermissions.Where(o => o.Group.Equals(group)))
                if ((requestedPermission & item.Exclusive) != 0)//اگر اجازه درخواستی حاوی اجازه ی جزیی ای است
                    if ((userPermissions & (item.Exclusive | item.Inclusive)) == item.Exclusive)//اگر اجازه های کاربر فقط اجازه جزیی را دارد و اجازه کامل آنرا شامل نمی شود
                        return true;

            return false;

        }

        public OT GetGroupObjectType(G group)
        {
            return dicGroupObjectTypeMaps[group];
        }

        public G GetGroupOfType(Type type)
        {
            return dicTypeGroupMaps[type];
        }

        public G[] GetObjectTypeGroups(OT objectType, bool directGroupsOnly = false)
        {
            if (directGroupsOnly)
                return dicGroupObjectTypeMaps.Where(o => o.Value.Equals(objectType)).Select(o => o.Key).ToArray();
            else
            {
                var allowedObjectTypes = dicObjectTypeParents.Where(o => o.Value.Contains(objectType)).Select(o => o.Key).ToArray().Union(new OT[] { objectType });
                return dicGroupObjectTypeMaps.Where(o => allowedObjectTypes.Contains(o.Value)).Select(o => o.Key).ToArray();
            }
        }

        public OT[] GetObjectTypeParents(OT objectType)
        {
            return dicObjectTypeParents[objectType] ?? new OT[0];
        }

        public OT[] GetObjectTypeChilds(OT objectType)
        {
            return dicObjectTypeParents.Where(o => o.Value.Contains(objectType)).Select(o => o.Key).ToArray();
        }

        public abstract Task<bool> HasPermissionAsync(int userId, PermissionObject<OT>? obj, G group, long permission, string permissionCondition = null);

        public override Task<bool> HasPermissionAsync(int userId, IPermissionObject obj, int group, long permission, string permissionCondition = null)
        {
            return HasPermissionAsync(userId, (PermissionObject<OT>?)obj, (G)(object)group, permission);
        }

        public Task<bool> HasPermissionAsync<P>(int userId, PermissionObject<OT>? obj, P permission, string permissionCondition = null)
        {
            int group = (int)(object)GetGroupOfType(typeof(P));
            long p = (long)(object)permission;

            return HasPermissionAsync(userId, obj, group, p, permissionCondition);
        }

        public bool HasPermission<P>(int userId, PermissionObject<OT>? obj, P permission, string permissionCondition = null)
        {
            return Task.Run(async () => await HasPermissionAsync(userId, obj, permission, permissionCondition)).Result;
        }

        public bool HasPermission(int userId, PermissionObject<OT>? obj, int group, long permission, string permissionCondition = null)
        {
            return Task.Run(async () => await HasPermissionAsync(userId, obj, group, permission, permissionCondition)).Result;
        }

        public bool HasAnyPermission<P>(int userId, P permission, string permissionCondition = null)
        {
            return Task.Run(async () => await HasPermissionAsync(userId, null, permission, permissionCondition)).Result;
        }

        public override bool HasAnyPermission(int userId, int group, long permission, string permissionCondition = null)
        {
            return Task.Run(async () => await HasPermissionAsync(userId, null, group, permission, permissionCondition)).Result;
        }

        public override bool HasPermission(int userId, IPermissionObject obj, int group, long permission, string permissionCondition = null)
        {
            return HasPermission(userId, PermissionObject<OT>.ReadFrom(obj), group, permission, permissionCondition);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="userId"></param>
        /// <param name="objectType"></param>
        /// <param name="permission"></param> 
        /// /// <param name="permissionCondition">در صورتیکه دسترسی با یک شرط کنترل می شود</param>
        /// <returns></returns>
        public int?[] GetAllowedObjectIds<P>(int userId, OT objectType, P permission, string permissionCondition = null)
        {
            var group = GetGroupOfType(typeof(P));
            return GetAllowedObjectIds(userId, objectType, (int)(object)group, (long)(object)permission, permissionCondition, null);
        }

        public int?[] GetAllowedObjectIds<P, E>(int userId, OT objectType, P permission, string permissionCondition, E extraData)
        {
            var group = GetGroupOfType(typeof(P));
            return GetAllowedObjectIds(userId, objectType, (int)(object)group, (long)(object)permission, permissionCondition, extraData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="objectType"></param>
        /// <param name="group"></param>
        /// <param name="permission"></param>
        /// <param name="permissionCondition">در صورتیکه دسترسی با یک شرط کنترل می شود</param>
        /// <param name="extraData">بیشتر داده های اختیاری است. مثلا اینکه تمام شناسه ها را بخواهیم با شناسه پدرانشان را با این پارامتر مشخص می کنیم</param>
        /// <returns></returns>
        protected abstract int?[] GetAllowedObjectIds(int userId, OT objectType, int group, long permission, string permissionCondition, object extraData = null);

        public int[] GetAllowedUserIds<P>(PermissionObject<OT> obj, P permission, string permissionCondition = null)
        {
            var group = GetGroupOfType(typeof(P));
            return GetAllowedUserIds(obj, (int)(object)group, (long)(object)permission, permissionCondition);
        }

        protected abstract int[] GetAllowedUserIds(PermissionObject<OT> obj, int group, long permission, string permissionCondition = null);



    }
}
