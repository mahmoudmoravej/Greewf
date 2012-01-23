using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Greewf.BaseLibrary.MVC.Logging.LogContext;

namespace Greewf.BaseLibrary.MVC.Logging
{
    public static class Logger
    {
        private static LogContext.LogContext context;


        public static void Log(Log log)
        {
            if (context == null)
                throw new Exception("LogConnectionString is not set for Logger. Set it before any log action");

            lock (context)
            {
                foreach (var item in log.LogDetails)
                    item.Id = Guid.NewGuid();

                context.Logs.Add(log);
                context.SaveChanges();
            }        
        }

        private static string logConnectionString;
        public static string LogConnectionString
        {
            get
            {
                return logConnectionString;
            }
            set
            {
                logConnectionString = value;
                lock (logConnectionString)
                {
                    context = new LogContext.LogContext(logConnectionString);
                }      
            }
        }
    }
}
