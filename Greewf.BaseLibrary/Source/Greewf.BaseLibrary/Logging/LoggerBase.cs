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

        public LoggerBase()
        {
            SaveCheckSum = true;
        }

        public abstract string Username { get; }
        public abstract string UserFullName { get; }

        protected abstract void ReadRequestToLog(ref Log log, object extraData);

        public bool SaveCheckSum { get; set; }

        public bool SaveUtcTime { get; set; }

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

        private void Log(Log log)
        {
            //توجه! به دلیل آنکه لازم است قبل از لاگ چک سام استخراج شود، لازم است که طول رشته ها متناسب شده باشند. در غیر اینصورت ممکن است رشته ای بعد از ذخیره ترانکیت شود و بعدا نمی توان آنرا ارزیابی کرد
            if (context == null)
                throw new Exception("LogConnectionString is not set for Logger. Set it before any log action");

            lock (context)
            {
                if (SaveCheckSum) log.Checksum = GetLogChecksum(log, "pjsf02347-9@#$%@#$%Fdskflnsdf@#%083SDF");
                context.Logs.Add(log);
                context.SaveChanges();
            }
        }

        public long Log(int logId, Type logEnumType, object model = null, Dictionary<string, string> modelDisplayNames = null, string[] exludeModelProperties = null, string text = null, object extraData = null)
        {
            if (LogProfileReader.Current.IsLogDisabled(logId, logEnumType)) return -1;
            var log = new Log();


            ReadRequestToLog(ref log, extraData);//NOTE : it may add log details too.
            if (!string.IsNullOrEmpty(text)) log.Text = text; //passed text is preferable to request text (if any)

            log.Browser = TakeMax(log.Browser, 50);
            log.UserAgent = TakeMax(log.UserAgent, 400);
            log.MachineName = TakeMax(log.MachineName, 50);
            log.RequestUrl = TakeMax(log.RequestUrl, 1000);

            log.Code = TakeMax(Enum.GetName(logEnumType, logId), 100);
            log.Text = TakeMax(model is Exception ? model.ToString() : log.Text, 4000);
            log.DateTime = SaveUtcTime ? DateTime.UtcNow : DateTime.Now;

            log.Username = TakeMax(ReadUsername(), 50);
            log.UserFullname = TakeMax(ReadUserFullName(), 50);

            if (log.ServerMachineName == null) FillServerDetails(log);

            if (model != null)
            {
                var typ = model.GetType();

                log.Key = TakeMax(typ.Name, 100);
                var excludedValues = AddLogDetails(log, model, modelDisplayNames, exludeModelProperties);//NOTE : log details may be filled previously in  ReadRequestToLog phase

                //remove excluded values from body too
                if (excludedValues != null && log.RequestBody != null)
                {
                    var body = log.RequestBody;
                    foreach (var exludedValue in excludedValues)
                        body = body.Replace(exludedValue, "***!!!EXCLUDED!!!***");

                    log.RequestBody = body;
                }
            }

            Log(log);
            return log.Id;

        }

        private void FillServerDetails(Logging.Log log)
        {
            log.ServerMachineName = TakeMax(System.Environment.MachineName, 40);

            try
            {
                var process = System.Diagnostics.Process.GetCurrentProcess();
                log.ServerProcessName = TakeMax(process.ProcessName, 30);
                log.ServerProcessId = process.Id;
            }
            catch
            {
            }
        }

        protected List<string> AddLogDetails(Log log, object model, Dictionary<string, string> modelDisplayNames = null, string[] exludeModelProperties = null)
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
                log.LogDetails.Add(new LogDetail
                {
                    Key = TakeMax((++idx).ToString(), 100),
                    Value = TakeMax(item == null ? "" : item.ToString(), 2000)
                });
        }

        private void AddDictionaryDetails(Log log, IDictionary arr)
        {
            foreach (var key in arr.Keys)
                log.LogDetails.Add(new LogDetail
                {
                    Key = TakeMax((key ?? new object()).ToString(), 100),
                    Value = TakeMax((arr[key] ?? new object()).ToString(), 2000)
                });
        }

        private string TakeMax(string str, int max)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return str.Substring(0, str.Length > max ? max : str.Length);
        }

        public bool IsLogChecksumOk(long logId)
        {
            var log = context.Logs.Where(o => o.Id == logId).First();
            return log.Checksum == GetLogChecksum(log, "pjsf02347-9@#$%@#$%Fdskflnsdf@#%083SDF");
        }


        private static int GetLogChecksum(Log log, string key)
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = (int)2166136261;

                //values for null are random constant
                hash = hash * 16777619 ^ GetTextHash(log.Browser ?? "b");
                hash = hash * 16777619 ^ GetTextHash(log.Code ?? "cz");
                hash = hash * 16777619 ^ GetDateHash(log.DateTime);
                hash = hash * 16777619 ^ GetBooleanHash(log.FromInternet ?? false);
                hash = hash * 16777619 ^ GetTextHash(log.Ip ?? "ir");
                hash = hash * 16777619 ^ GetBooleanHash(log.IsMobile ?? true);
                hash = hash * 16777619 ^ GetTextHash(log.Key ?? "tg");
                hash = hash * 16777619 ^ GetTextHash(log.MachineName ?? "w");
                hash = hash * 16777619 ^ GetTextHash(key ?? "zzket");
                hash = hash * 16777619 ^ GetTextHash(log.Querystring ?? "ll");
                hash = hash * 16777619 ^ GetTextHash(log.RequestBody ?? "v");
                hash = hash * 16777619 ^ GetTextHash(log.RequestHeaders ?? "vf");
                hash = hash * 16777619 ^ GetTextHash(log.RequestMethod ?? "54");
                hash = hash * 16777619 ^ GetTextHash(log.RequestUrl ?? "^%");
                hash = hash * 16777619 ^ GetTextHash(log.Text ?? "$|");
                hash = hash * 16777619 ^ GetTextHash(log.UserAgent ?? "$|");
                hash = hash * 16777619 ^ GetTextHash(log.UserFullname ?? "fd");
                hash = hash * 16777619 ^ GetTextHash(log.Username ?? "V*");
                hash = hash * 16777619 ^ GetTextHash(log.ServerMachineName ?? "Sm$*");
                hash = hash * 16777619 ^ GetTextHash(log.ServerProcessName ?? "sp^");
                hash = hash * 16777619 ^ GetIntHash(log.ServerProcessId ?? -999999);

                if (log.LogDetails != null)
                    foreach (var detail in log.LogDetails)
                    {
                        hash = hash * 16777619 ^ GetTextHash(detail.Key ?? "&y");
                        hash = hash * 16777619 ^ GetTextHash(detail.KeyTitle ?? "4jfej");
                        hash = hash * 16777619 ^ GetTextHash(detail.Value ?? "@vken");
                    }

                return hash;
            }
        }

        private static int GetTextHash(string text)
        {
            //based on http://stackoverflow.com/a/5155015/790811
            if (text == null) text = "";

            unchecked
            {
                int hash = 23;
                foreach (char c in text)
                {
                    hash = hash * 31 + c;
                }
                return hash;
            }

        }

        private static int GetDateHash(DateTime date)
        {
            //it is safe if your column type is datetime2. Are you sure?!! if not please change it to : .ToString("yyyMMddHHmmssfff")            
            return GetTextHash(date.Ticks.ToString());

            //return GetTextHash(date.ToString("MMddyyyyHHmmssfff"));
        }

        private static int GetBooleanHash(bool value)
        {
            return value ? 937097297 : 0389803085;
        }

        private static int GetIntHash(int value)
        {
            return value;
        }

        public int GetAllLgosCount()
        {
            if (context == null)
                throw new Exception("LogConnectionString is not set for Logger. Set it before any log action");


            return context.Logs.Count();
        }

    }




}
