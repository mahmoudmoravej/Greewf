using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Greewf.BaseLibrary.MVC.Logging.LogContext;
using System.Web;
using System.Web.Mvc;
using System.Collections;

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
                context.Logs.Add(log);
                context.SaveChanges();
            }
        }

        public void Log<T>(T logId, object model, string[] exludeModelProperties) where T : struct
        {
            var typ = typeof(T);
            Log((int)Convert.ChangeType(logId, typ), typ, model, exludeModelProperties);
        }

        public void Log<T>(T logId, object model = null, ModelMetadata modelMetadata = null, string[] exludeModelProperties = null) where T : struct
        {
            var typ = typeof(T);
            Log((int)Convert.ChangeType(logId, typ), typ, model, modelMetadata, exludeModelProperties);
        }

        public void Log(int logId, Type logEnumType, object model, string[] exludeModelProperties = null)
        {
            var metaData = ModelMetadataProviders.Current.GetMetadataForType(() => { return model; }, model.GetType());
            Log(logId, logEnumType, model, metaData, exludeModelProperties);
        }

        public void Log(int logId, Type logEnumType, object model = null, ModelMetadata modelMetadata = null, string[] exludeModelProperties = null)
        {
            if (LogProfileReader.Current.IsLogDisabled(logId, logEnumType)) return;
            var log = new Log();
            var request = HttpContext.Current.Request;

            log.DateTime = DateTime.Now;
            log.Browser = TakeMax(request.Browser.Browser + request.Browser.Version, 50);
            log.IsMobile = request.Browser.IsMobileDevice;
            log.UserAgent = TakeMax(request.UserAgent, 150);
            log.Code = TakeMax(Enum.GetName(logEnumType, logId), 50);
            log.Text = (model is Exception ? TakeMax(model.ToString(), 4000) : null);//TODO : for future use!
            log.Ip = request.UserHostAddress;
            log.MachineName = TakeMax(request.UserHostName, 50);
            log.Username = TakeMax(Username, 50);
            log.UserFullname = TakeMax(UserFullName, 50);
            log.RequestUrl = TakeMax(request.Url.GetLeftPart(UriPartial.Path), 150);
            log.Querystring = TakeMax(request.QueryString.ToString(), 200);

            if (model != null)
            {
                var typ = model.GetType();
                if (modelMetadata == null)
                    modelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => { return model; }, typ);

                log.Key = TakeMax(typ.Name, 30);
                AddLogDetails(log, model, modelMetadata, exludeModelProperties);
            }

            Log(log);

        }

        private void AddLogDetails(LogContext.Log log, object model, ModelMetadata modelMetadata = null, string[] exludeModelProperties = null)
        {
            if (model is IDictionary)
                AddDictionaryDetails(log, model as IDictionary);
            else if (model is IEnumerable)
                AddArrayDetails(log, model as IEnumerable);
            else
                AddObjectDetails(log, model, modelMetadata, exludeModelProperties);

        }

        private void AddObjectDetails(LogContext.Log log, object model, ModelMetadata modelMetadata = null, string[] exludeModelProperties = null)
        {
            foreach (var item in model.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty))
            {
                if (!(item.PropertyType == typeof(String) || item.PropertyType.IsValueType)) continue;

                var key = item.Name;

                if (exludeModelProperties != null && exludeModelProperties.Contains(key)) continue;
                var logDetail = new LogDetail();
                logDetail.Key = TakeMax(key, 100);

                //key
                if (modelMetadata != null)
                {
                    var meta = modelMetadata.Properties.SingleOrDefault(o => o.PropertyName == key);
                    if (meta != null && meta.DisplayName != null)
                        logDetail.KeyTitle = TakeMax(meta.DisplayName, 200);
                }

                //value
                object value = item.GetValue(model, null);
                logDetail.Value = value == null ? "" : TakeMax(value.ToString(), 2000);

                log.LogDetails.Add(logDetail);
            }
        }

        private void AddArrayDetails(LogContext.Log log, IEnumerable arr)
        {
            int idx = 0;
            foreach (var item in arr)
                log.LogDetails.Add(new LogDetail { Key = (++idx).ToString(), Value = item.ToString() });
        }

        private void AddDictionaryDetails(LogContext.Log log, IDictionary arr)
        {
            foreach (var key in arr.Keys)
                log.LogDetails.Add(new LogDetail { Key = (key ?? new object()).ToString(), Value = (arr[key] ?? new object()).ToString() });
        }


        private string TakeMax(string str, int max)
        {
            return str.Substring(0, str.Length > max ? max : str.Length);
        }


    }




}
