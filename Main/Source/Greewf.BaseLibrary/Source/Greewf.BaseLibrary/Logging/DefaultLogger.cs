using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections;

namespace Greewf.BaseLibrary.Logging
{

    public class DefaultLogger : LoggerBase
    {

        public override string Username
        {
            get { return null; }
        }

        public override string UserFullName
        {
            get { return null; }
        }

        protected override void ReadRequestToLog(ref Log log, object extraData)
        {
            return;
        }
    }




}
