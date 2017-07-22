using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Profile;

namespace Greewf.BaseLibrary.MVC
{
    public abstract class ProfileCommonBase : ProfileBase
    {

        public ProfileCommonBase()
        {

        }

        public virtual string FirstName
        {
            get
            {
                return ((string)(this.GetPropertyValue("FirstName")));
            }
            set
            {
                this.SetPropertyValue("FirstName", value);
            }
        }
        public virtual string LastName
        {
            get
            {
                return ((string)(this.GetPropertyValue("LastName")));
            }
            set
            {
                this.SetPropertyValue("LastName", value);
            }
        }

        public override string ToString()
        {
            return ToString(false,true);
        }

        public string ToString(bool isShort,bool familyFirst)
        {
            string f, l;
            if (familyFirst)
            {
                f = LastName;
                l = FirstName;
            }
            else
            {
                f = FirstName;
                l = LastName;
            }

            if (isShort)
                return string.Format("{0} {1}", f, l);
            else
                return string.Format("{0} {1} ({2})", f, l, UserName);
        }

        public static ProfileCommonBase GetProfile(string username)
        {
            return Create(username) as ProfileCommonBase;
        }
         


    }


}