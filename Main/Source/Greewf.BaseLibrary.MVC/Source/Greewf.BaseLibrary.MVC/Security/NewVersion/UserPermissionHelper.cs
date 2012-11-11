using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Greewf.BaseLibrary.MVC.Security
{

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="C">Permission Category</typeparam>
    /// <typeparam name="K">Category Object Key</typeparam>
    public struct UserAclKey<C, K>
        where K : struct
        where C : struct
    {
        public C? Category;
        public K? CategoryKey;

        public UserAclKey(C? category, K? categoryKey)
        {
            Category = category;
            CategoryKey = categoryKey;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            UserAclKey<C, K> p = (UserAclKey<C, K>)obj;

            // Return true if the fields match:
            if ((Category == null && p.CategoryKey == null) || (Category != null && Category.Equals(p.Category)))
                if (CategoryKey == null && p.CategoryKey == null)
                    return true;
                else if (CategoryKey != null && CategoryKey.Equals(p.CategoryKey))
                    return true;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="P">PermissionEntity enum</typeparam>
    /// <typeparam name="C">PermissionCategory enum</typeparam>
    /// <typeparam name="PC">Permission coordinator class</typeparam>
    /// <typeparam name="K">Category Object Key(int or guid in most cases)</typeparam>
    public sealed class UserPermissionHelper<P, C, PC, K>
        where P : struct
        where PC : PermissionCoordinatorBase<P, C>
        where C : struct
        where K : struct
    {

        public delegate Dictionary<UserAclKey<C, K>, Dictionary<P, long>> UserAclRetreiverDelegate();
        public UserAclRetreiverDelegate UserAclRetreiver { get; set; }
        public Func<bool> IsEnterpriseAdmin;

        public PC PermissionCoordinator
        {
            get;
            set;
        }


        private UserAclRetreiverDelegate UserAcl
        {
            get
            {
                if (UserAclRetreiver == null)
                    throw new Exception("UserAclRetreiver property has not be defined");
                return UserAclRetreiver;

            }
        }

        private Dictionary<P, long> GetCategoryAcl(P permission, K? categoryKey)
        {
            var acl = UserAcl();
            var cat = PermissionCoordinator.GetPermissionCategory(permission);

            if (cat != null && categoryKey == null)
                throw new Exception("The requested permission object[" + typeof(P).ToString() + "] has category[" + cat.Value.ToString() + "], but you passed a null categoryKey! This permission is only valid within an instance of a category.");

            if (cat == null && categoryKey != null)//TODO : we can bypass this instead of exception throwing...
                throw new Exception("The requested permission object[" + typeof(P).ToString() + "] has no any category, but you passed categoryKey[" + categoryKey.ToString() + "]!");

            var key = new UserAclKey<C, K>(cat, categoryKey);
            if (acl.ContainsKey(key))
                return acl[key];
            return null;//مثلا زمانیکه در رسته استان تهران  کلا دسترسی ای برای ایشان تعریف نشده است
        }

        public bool HasPermission<T>(T permissions, K? categoryKey = null) where T : struct
        {
            var entityItem = PermissionCoordinator.GetRelatedPermissionItem(typeof(T));

            return HasPermission(entityItem, Convert.ToInt64(permissions), categoryKey: categoryKey) ?? false;
        }

        /// <summary>
        /// بدون توجه به رسته اجازه ها، چک می کند که آیا اجازه ای دارد. برای زمانی مناسب است که مثلا می خواهیم یک منو رو فعال یا غیرفعال کنیم ولی رسته ان بعدا مشخص می شود
        /// مثال : آیا اجازه ارسال دستور حداقل در یک استان را دارد؟
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public bool HasAnyCategoryPermission<T>(T permissions) where T : struct
        {
            if (IsEnterpriseAdmin != null && IsEnterpriseAdmin()) return true;
            var entityItem = PermissionCoordinator.GetRelatedPermissionItem(typeof(T));
            return HasAnyCategoryPermission(entityItem, Convert.ToInt64(permissions), null);
        }

        public bool HasAnyCategoryPermission(P permissionObject, long permissions, PermissionLimiterBase limiterFunctionChecker)
        {
            if (IsEnterpriseAdmin != null && IsEnterpriseAdmin()) return true;

            var acls = UserAcl();

            foreach (var acl in acls.Values)
            {
                if (HasPermissionInAcl(permissionObject, permissions, limiterFunctionChecker, acl) == true)
                    return true;
            }
            return false;

        }

        /// <summary>
        /// مقدار نال به معنی این است که بر روی همه اجازه دارد و تنها برای مدیر ارشد این موضوع معنا دارد
        /// برای سایرین لیست خالی یا لیست با مقدار بازگردانده می شود
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public List<K?> GetAllowedCategoryObjects<T>(T permissions) where T : struct
        {
            if (IsEnterpriseAdmin != null && IsEnterpriseAdmin()) return null;
            var entityItem = PermissionCoordinator.GetRelatedPermissionItem(typeof(T));
            var acls = UserAcl();
            var cat = PermissionCoordinator.GetPermissionCategory(entityItem);
            var lst = new List<K?>();

            foreach (var aclItem in acls)
            {
                if (aclItem.Key.Category.Equals(cat))
                {
                    if (HasPermissionInAcl(entityItem, Convert.ToInt64(permissions), null, aclItem.Value) == true)
                        lst.Add(aclItem.Key.CategoryKey);
                }
            }
            return lst;

        }

        public bool? HasPermission(P permissionObject, long requestedPermissions/*NOTE:this parameter can be cumulative: OR base not AND base*/, PermissionLimiterBase permissionLimiter = null, K? categoryKey = null)
        {

            if (IsEnterpriseAdmin != null && IsEnterpriseAdmin()) return true;
            var acl = GetCategoryAcl(permissionObject, categoryKey);

            return HasPermissionInAcl(permissionObject, requestedPermissions, permissionLimiter, acl);
        }

        private bool? HasPermissionInAcl(P permissionObject, long requestedPermissions, PermissionLimiterBase permissionLimiter, Dictionary<P, long> acl)
        {
            if (acl == null) return false;//سطوح دسترسی برای موضوع مورد نظر تعریف نشده است
            if (acl.ContainsKey(permissionObject))
            {

                //NOTE : اجازه درخواستی نمی بایست "فول" را داشته باشد. البته "اور" شده جزیی ها را می تواند داشته باشد
                //       در واقع هیچگاه "فول" ها که فقط در دیتابیس معنی دارند نمی بایست بطور مجزا در "ریکوستت پرمیشن" حاضر شوند
                //       لذا ممکن است که در اجازه های کاربر(یا همان "یوزر پرمیشن") یک "فول" وجود داشته باشد اما هرگز در "اجازه های موردنیاز" یک "فول" بطور تنها نیست و "جزیی" های آن را نیز شامل می شود
                long userPermissions = acl[permissionObject];
                long availablePermissions = userPermissions & (long)requestedPermissions;
                bool result = availablePermissions != 0;

                if (result &&
                    permissionLimiter != null &&
                    PermissionCoordinator.HasOnlySubPermission(permissionObject, requestedPermissions, userPermissions)) // برای اجازه های کامل فراخوانی توابع محدود شده بی معنی است و اصلا نباید این توابع صدا زده شوند. بطور پیش فرض هم تمامی اجازه ها فول هستند مگر آنکه قبلا تعریف شده باشند
                {
                    // در صورتیکه اجازه موردنیاز با دسترسی کاربر تطابق داشت و تطبیق آن با شرایط اجرای تابع همخوانی داشت آنگاه تابع را فراخوانی کن
                    // توجه : خالی بودن "limitterPermission" خطرناک است و وضعیت را مبهم میکند
                    if (permissionLimiter.LimiterPermission != null)//در صورتیکه تابع محدود شده مخصوص اجازه های خاصی بود
                        availablePermissions = availablePermissions & permissionLimiter.LimiterPermission.Value;

                    result = availablePermissions != 0;
                    if (result == false)
                        return null;

                    return permissionLimiter.LimiterFunction();
                }
                return result;
            }

            return false;
        }

        public bool HasFullPermissionOf<T>(T permissions, K? categoryKey = null) where T : struct
        {

            var entityItem = PermissionCoordinator.GetRelatedPermissionItem(typeof(T));

            return HasFullPermissionOf(entityItem, Convert.ToInt64(permissions), categoryKey);

        }

        /// <summary>
        /// از ارسال اجازه های ترکیبی به این پارامتر خودداری شود
        /// </summary>
        /// <param name="permissionObject"></param>
        /// <param name="requestedPermission"></param>
        /// <returns></returns>
        public bool HasFullPermissionOf(P permissionObject, long requestedPermission, K? categoryKey = null)
        {
            if (IsEnterpriseAdmin != null && IsEnterpriseAdmin()) return true;
            var acl = GetCategoryAcl(permissionObject, categoryKey);
            if (acl != null && acl.ContainsKey(permissionObject))
            {
                long userPermissions = acl[permissionObject];
                return !PermissionCoordinator.HasOnlySubPermission(permissionObject, requestedPermission, userPermissions);
            }
            return false;
        }

    }
}
