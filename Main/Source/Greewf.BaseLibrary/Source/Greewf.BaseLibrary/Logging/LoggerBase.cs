using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections;

namespace Greewf.BaseLibrary.Logging
{

    public abstract class LoggerBase
    {

        public abstract string Username { get; }
        public abstract string UserFullName { get; }

        protected abstract void ReadRequestToLog(ref Log log, object extraData);


        /// <summary>
        /// it is provided to handle exception. this function should never throw an exception
        /// </summary>
        /// <returns></returns>
        protected virtual string ReadUsername()
        {
            try
            {
                return Username;
            }
            catch (Exception x)
            {
                return "Exception: " + x.ToString();
            }

        }

        /// <summary>
        /// it is provided to handle exception. this function should never throw an exception
        /// </summary>
        /// <returns></returns>
        protected virtual string ReadUserFullName()
        {
            try
            {
                return UserFullName;
            }
            catch (Exception x)
            {
                return "Exception: " + x.ToString();
            }

        }

        private LogContext context;

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
                    context = new LogContext(logConnectionString);
                }
            }
        }

        public void Log(Log log)
        {
            if (context == null)
                throw new Exception("LogConnectionString is not set for Logger. Set it before any log action");

            lock (context)
            {
                context.Logs.Add(log);
                context.SaveChanges();
            }
        }

        public long Log(int logId, Type logEnumType, object model = null, Dictionary<string, string> modelDisplayNames = null, string[] exludeModelProperties = null, string text = null, object extraData = null)
        {
            if (LogProfileReader.Current.IsLogDisabled(logId, logEnumType)) return -1;
            var log = new Log();


            ReadRequestToLog(ref log, extraData);
            if (!string.IsNullOrEmpty(text)) log.Text = text; //passed text is preferable to request text (if any)

            log.Browser = TakeMax(log.Browser, 50);
            log.UserAgent = TakeMax(log.UserAgent, 400);
            log.MachineName = TakeMax(log.MachineName, 50);
            log.RequestUrl = TakeMax(log.RequestUrl, 1000);

            log.Code = TakeMax(Enum.GetName(logEnumType, logId), 100);
            log.Text = TakeMax(model is Exception ? model.ToString() : log.Text, 4000);
            log.DateTime = DateTime.Now;

            log.Username = TakeMax(ReadUsername(), 50);
            log.UserFullname = TakeMax(ReadUserFullName(), 50);

            if (model != null)
            {
                var typ = model.GetType();

                log.Key = TakeMax(typ.Name, 100);
                var excludedValues = AddLogDetails(log, model, modelDisplayNames, exludeModelProperties);

                //remove excluded values from body too
                if (excludedValues != null && log.RequestBody != null)
                    foreach (var exludedValue in excludedValues)
                        log.RequestBody = log.RequestBody.Replace(exludedValue, "***!!!EXCLUDED!!!***");
            }

            Log(log);
            return log.Id;

        }

        private List<string> AddLogDetails(Log log, object model, Dictionary<string, string> modelDisplayNames = null, string[] exludeModelProperties = null)
        {
            List<string> excludedValues = null;

            if (model is IDictionary)
                AddDictionaryDetails(log, model as IDictionary);
            else if (model is IEnumerable)
                AddArrayDetails(log, model as IEnumerable);
            else
                AddObjectDetails(log, model, modelDisplayNames, exludeModelProperties, out excludedValues);

            return excludedValues;
        }

        private void AddObjectDetails(Log log, object model, Dictionary<string, string> modelDisplayNames, string[] exludeModelProperties, out List<string> excludedValues)
        {
            excludedValues = new List<string>();

            foreach (var item in model.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty))
            {
                if (!(item.PropertyType == typeof(String) || item.PropertyType.IsValueType)) continue;

                var key = item.Name;
                object value = item.GetValue(model, null);
                string valueString = value == null ? null : value.ToString();

                if (exludeModelProperties != null && exludeModelProperties.Contains(key))
                {
                    if (!string.IsNullOrEmpty(valueString)) excludedValues.Add(value.ToString());
                    continue;
                }

                var logDetail = new LogDetail();
                logDetail.Key = TakeMax(key, 100);

                //key
                if (modelDisplayNames != null && modelDisplayNames.ContainsKey(key))
                {
                    var displayName = modelDisplayNames[key];
                    if (displayName != null)
                        logDetail.KeyTitle = TakeMax(displayName, 200);
                }

                //value                
                logDetail.Value = value == null ? "" : TakeMax(valueString, 2000);

                log.LogDetails.Add(logDetail);
            }
        }
        private void AddArrayDetails(Log log, IEnumerable arr)
        {
            int idx = 0;
            foreach (var item in arr)
                log.LogDetails.Add(new LogDetail { Key = (++idx).ToString(), Value = item.ToString() });
        }

        private void AddDictionaryDetails(Log log, IDictionary arr)
        {
            foreach (var key in arr.Keys)
                log.LogDetails.Add(new LogDetail { Key = (key ?? new object()).ToString(), Value = (arr[key] ?? new object()).ToString() });
        }

        private string TakeMax(string str, int max)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return str.Substring(0, str.Length > max ? max : str.Length);
        }


    }




}
