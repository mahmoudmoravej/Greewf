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
      

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="P">PermissionEntity enum</typeparam>
    /// <typeparam name="C">PermissionCategory enum</typeparam>
    /// <typeparam name="PC">Permission coordinator class</typeparam>
    /// <typeparam name="K">Category Object Key(int or guid in most cases)</typeparam>
    public sealed class UserPermissionHelper<P, C, PC, K>
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
                throw new Exception("The requested permission object[" + typeof(P).ToString() + "] has category["+cat.Value.ToString() +"], but you passed a null categoryKey! This permission is only valid within an instance of a category.");

            if (cat == null && categoryKey != null)//TODO : we can bypass this instead of exception throwing...
                throw new Exception("The requested permission object["+ typeof(P).ToString() +"] has no any category, but you passed categoryKey["+categoryKey.ToString()+"]!");

            return acl[new UserAclKey<C, K>(cat, categoryKey)];
        }

        public bool HasPermission<T>(T permissions, K? categoryKey = null) where T : struct
        {
            var entityItem = PermissionCoordinator.GetRelatedPermissionItem(typeof(T));

            return HasPermission(entityItem, Convert.ToInt64(permissions), categoryKey: categoryKey) ?? false;
        }



        public bool? HasPermission(P permissionObject, long requestedPermissions/*NOTE:this parameter can be cumulative: OR base not AND base*/, PermissionLimiterBase permissionLimiter = null, K? categoryKey = null)
        {

            if (IsEnterpriseAdmin != null && IsEnterpriseAdmin()) return true;
            var acl = GetCategoryAcl(permissionObject, categoryKey);

            if (acl.ContainsKey(permissionObject))
            {

                //NOTE : اجازه درخواستی نمی بایست "فول" را داشته باشد. البته "اور" شده ها را می تواند داشته باشد
                //       در واقع هیچگاه "فول" ها که فقط در دیتابیس معنی دارند نمی بایست بطور مجزا در "ریکوستت پرمیشن" حاضر شوند
                //       لذا ممکن است که در اجازه های کاربر یک "فول" وجود داشته باشد اما هرگز در "اجازه های موردنیاز" یک "فول" بطور تنها نیست و "محدود کننده" های آن را نیز شامل می شود
                long userPermissions = acl[permissionObject];
                long availablePermissions = userPermissions & (long)requestedPermissions;
                bool result = availablePermissions != 0;

                if (result &&
                    permissionLimiter != null &&
                    PermissionCoordinator.IsLimitedPermission(permissionObject, requestedPermissions, userPermissions)) // برای اجازه های فول فراخوانی توابع محدود کننده بی معنی است و اصلا نباید این توابع صدا زده شوند. بطور پیش فرض هم تمامی اجازه ها فول هستند مگر آنکه قبلا تعریف شده باشند
                {
                    // در صورتیکه اجازه موردنیاز با دسترسی کاربر تطابق داشت و تطبیق آن با شرایط اجرای تابع همخوانی داشت آنگاه تابع را فراخوانی کن
                    // توجه : خالی بودن "limitterPermission" خطرناک است و وضعیت را مبهم میکند
                    if (permissionLimiter.LimiterPermission != null)//در صورتیکه تابع محدود کننده مخصوص اجازه های خاصی بود
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
        /// <param name="requestedPermissions"></param>
        /// <returns></returns>
        public bool HasFullPermissionOf(P permissionObject, long requestedPermissions, K? categoryKey = null)
        {
            if (IsEnterpriseAdmin != null && IsEnterpriseAdmin()) return true;
            var acl = GetCategoryAcl(permissionObject, categoryKey);
            if (acl.ContainsKey(permissionObject))
            {
                long userPermissions = acl[permissionObject];
                return !PermissionCoordinator.IsLimitedPermission(permissionObject, requestedPermissions, userPermissions);
            }
            return false;
        }

    }
}
