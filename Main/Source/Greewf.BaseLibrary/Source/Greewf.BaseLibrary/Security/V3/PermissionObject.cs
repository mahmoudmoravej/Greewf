using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greewf.BaseLibrary.Security.V3
{

    public interface IPermissionObject
    {
        int GetObjectType();
        int? GetId();

    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="OT">Permission Object Type</typeparam>
    public struct PermissionObject<OT> : IPermissionObject
        where OT : struct
    {

        public OT Type;
        public int? Id;


        public int GetObjectType()
        {
            return (int)(object)Type;
        }

        public int? GetId()
        {
            return Id;
        }

        public static PermissionObject<OT> ReadFrom(IPermissionObject source)
        {
            var obj = new PermissionObject<OT>();
            obj.Id = source.GetId();
            obj.Type = (OT)(object)source.GetObjectType();
            return obj;
        }

        
    }
}
