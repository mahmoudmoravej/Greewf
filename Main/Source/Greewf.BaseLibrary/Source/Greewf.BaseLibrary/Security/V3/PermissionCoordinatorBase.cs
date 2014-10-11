using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Greewf.BaseLibrary.Security.V3
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="G">Permission Group</typeparam>
    /// <typeparam name="OT">Permission Object Type</typeparam>
    public abstract class PermissionCoordinatorBase<OT, G>
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

        private static PermissionCoordinatorBase<G, OT> _instance = null;
        public static T GetInstance<T>() where T : PermissionCoordinatorBase<G, OT>, new()
        {
            if (_instance == null)
                _instance = new T();

            var result = _instance as T;
            if (result == null)
                throw new Exception(string.Format("This method can be called only with the type that was passed to it first call. It is associated with '{0}' previously." + typeof(T).FullName));

            return result;
        }

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
            catch (KeyNotFoundException x)
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

        public G[] GetObjectTypeGroups(OT objectType)
        {
            return dicGroupObjectTypeMaps.Where(o => o.Value.Equals(objectType)).Select(o => o.Key).ToArray();
        }

        public OT[] GetObjectTypeParents(OT objectType)
        {
            return dicObjectTypeParents[objectType] ?? new OT[0];
        }

        public abstract  Task<bool> HasPermissionAsync<P>(int userId, OT objectType, int? objectId, P permission);

        public bool HasPermission<P>(int userId, OT objectType, int? objectId, P permission)
        {
            return Task.Run(async () => await HasPermissionAsync(userId, objectType, objectId, permission)).Result;
        }
    }
}
