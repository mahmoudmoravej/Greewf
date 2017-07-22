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
    public class RelatedPermission<G>
        where G : struct
    {
        /// <summary>
        /// اجازه کامل 
        /// </summary>
        public long Inclusive = -1;
        public long Exclusive = -1;
        public G Group;

        public RelatedPermission(G group, long exclusive, long inclusive)
        {
            Group = group;
            Exclusive = exclusive;
            Inclusive = inclusive;
        }

    }
}
