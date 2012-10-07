using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Greewf.BaseLibrary.MVC.Security
{

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="P">PermissionEntity enum</typeparam>
    public sealed class UserPermissionHelper<P, C>
        where C : PermissionCoordinatorBase<P>
    {

        public delegate Dictionary<P, long> UserAclRetreiverDelegate();
        public UserAclRetreiverDelegate UserAclRetreiver { get; set; }
        public Func<bool> IsEnterpriseAdmin;

        public C PermissionCoordinator
        {
            get;
            set;
        }


        private UserAclRetreiverDelegate UserAcl
        {
            get
            {
                if (UserAclRetreiver == null)
                    throw new Exception("UserAclRetreiver property should not be defined");
                return UserAclRetreiver;

            }
        }

        public bool HasPermission<T>(T permissions) where T : struct
        {
            var entityItem = PermissionCoordinator.GetRelatedPermissionItem(typeof(T));

            return HasPermission(entityItem, Convert.ToInt64(permissions)) ?? false;
        }

        public bool? HasPermission(P permissionObject, long requestedPermissions/*NOTE:this parameter can be cumulative: OR base not AND base*/, PermissionLimiterBase permissionLimiter = null)
        {

            if (IsEnterpriseAdmin != null && IsEnterpriseAdmin()) return true;

            var acl = UserAcl();
            if (acl.ContainsKey(permissionObject))
            {

                //NOTE : اجازه درخواستی نمی بایست "فول" را داشته باشد. البته "اور" شده ها را می تواند داشته باشد
                //       در واقع هیچگاه "فول" ها که فقط در دیتابیس معنی دارند نمی بایست بطور مجزا در "ریکوستت پرمیشن" حاضر شوند
                //       لذا ممکن است که در اجازه های کاربر یک "فول" وجود داشته باشد اما هرگز در "اجازه های موردنیاز" یک "فول" بطور تنها نیست و "محدود شده" های آن را نیز شامل می شود
                long userPermissions = acl[permissionObject];
                long availablePermissions = userPermissions & (long)requestedPermissions;
                bool result = availablePermissions != 0;

                if (result &&
                    permissionLimiter != null &&
                    PermissionCoordinator.IsLimitedPermission(permissionObject, requestedPermissions, userPermissions)) // برای اجازه های فول فراخوانی توابع محدود شده بی معنی است و اصلا نباید این توابع صدا زده شوند. بطور پیش فرض هم تمامی اجازه ها فول هستند مگر آنکه قبلا تعریف شده باشند
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

        public bool HasFullPermissionOf<T>(T permissions) where T : struct
        {

            var entityItem = PermissionCoordinator.GetRelatedPermissionItem(typeof(T));

            return HasFullPermissionOf(entityItem, Convert.ToInt64(permissions));

        }

        /// <summary>
        /// از ارسال اجازه های ترکیبی به این پارامتر خودداری شود
        /// </summary>
        /// <param name="permissionObject"></param>
        /// <param name="requestedPermissions"></param>
        /// <returns></returns>
        public bool HasFullPermissionOf(P permissionObject, long requestedPermissions)
        {
            if (IsEnterpriseAdmin != null && IsEnterpriseAdmin()) return true;
            var acl = UserAcl();
            if (acl.ContainsKey(permissionObject))
            {
                long userPermissions = acl[permissionObject];
                return !PermissionCoordinator.IsLimitedPermission(permissionObject, requestedPermissions, userPermissions);
            }
            return false;
        }

    }
   
}
