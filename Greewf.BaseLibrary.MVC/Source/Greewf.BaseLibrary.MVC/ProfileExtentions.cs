using System.Collections.Generic;
using System.Web.Profile;

namespace Greewf.BaseLibrary.MVC
{
    public static class ProfileExtentions
    {
        public static IEnumerable<T> ToProfileCommonEnumerable<T>(this ProfileInfoCollection sender) where T : ProfileCommonBase
        {
            foreach (ProfileInfo item in sender)
                yield return ProfileCommonBase.GetProfile(item.UserName) as T;
        }

    }
}