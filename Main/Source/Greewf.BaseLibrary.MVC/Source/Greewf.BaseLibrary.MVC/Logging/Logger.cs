using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Greewf.BaseLibrary.MVC.Logging.LogContext;
using System.Web;
using System.Web.Mvc;

namespace Greewf.BaseLibrary.MVC.Logging
{
    public abstract class Logger
    {
        private static Logger current = new DefaultLogger();
        public static Logger Current
        {
            get
            {
                return current;
            }
            set
            {
                if (value == null)
                    current = new DefaultLogger();
                else
                    current = value;
            }
        }

        public abstract string Username { get; }
        public abstract string UserFullName { get; }

        private LogContext.LogContext context;

        private string logConnectionString;
        public string LogConnectionString
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

        private void Log(Log log)
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

        public void Log<T>(T logId, object model = null, ModelMetadata modelMetadata = null, string[] exludeModelProperties = null) where T : struct
        {
            var typ = typeof(T);
            Log((int)Convert.ChangeType(logId, typ), typ, model, modelMetadata, exludeModelProperties);
        }

        public void Log(int logId, Type logEnumType, object model = null, ModelMetadata modelMetadata = null, string[] exludeModelProperties=null)
        {
            var log = new Log();
            var request = HttpContext.Current.Request;

            log.DateTime = DateTime.Now;
            log.Browser = request.Browser.Browser + request.Browser.Version;
            log.IsMobile = request.Browser.IsMobileDevice;
            log.UserAgent = request.UserAgent;
            log.Code = Enum.GetName(logEnumType, logId);
            log.Text = logId.ToString();
            log.Ip = request.UserHostAddress;
            log.MachineName = request.UserHostName;
            log.Username = Username;
            log.UserFullname = UserFullName;
            log.RequestUrl = request.Url.ToString();
            log.Querystring = request.QueryString.ToString();

            if (model != null)
            {
                log.Key = model.GetType().Name;
                AddLogDetails(log, model, modelMetadata, exludeModelProperties);
            }

            Log(log);

        }

        private void AddLogDetails(LogContext.Log log, object model, ModelMetadata modelMetadata = null, string[] exludeModelProperties=null)
        {
            foreach (var item in model.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty))
            {
                if (!(item.PropertyType == typeof(String) || item.PropertyType.IsValueType)) continue;
                
                var key=item.Name;

                if (exludeModelProperties.Contains(key)) continue;
                var logDetail = new LogDetail();

                //key
                if (modelMetadata == null)
                    logDetail.Key = key ;
                else
                {
                    var meta = modelMetadata.Properties.SingleOrDefault(o => o.PropertyName == key);
                    if (meta != null)
                        logDetail.Key = meta.DisplayName ?? key;
                    else
                        logDetail.Key = key;
                }

                //value
                object value = item.GetValue(model, null);
                logDetail.Value = value == null ? "" : value.ToString();

                log.LogDetails.Add(logDetail);
            }


        }

    }

}
