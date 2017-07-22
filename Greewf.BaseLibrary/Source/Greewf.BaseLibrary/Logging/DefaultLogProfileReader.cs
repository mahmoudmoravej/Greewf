using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greewf.BaseLibrary.Logging
{
    public class DefaultLogProfileReader : LogProfileReader
    {
        public DefaultLogProfileReader(string logProfileFilePath, string logPointFilePath)
            : base(logProfileFilePath, logPointFilePath)
        {            
        }

    }
}
