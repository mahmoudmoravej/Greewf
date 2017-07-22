//Source : http://stackoverflow.com/a/26762756/790811

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greewf.BaseLibrary.Linq
{
    public interface IQueryHintContext
    {
        string QueryHint { get; set; }
        bool ApplyHint { get; set; }
    }
}
