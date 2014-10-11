using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Greewf.BaseLibrary.Security
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

 
}
