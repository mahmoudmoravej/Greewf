using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Web.Caching;

namespace Greewf.BaseLibrary.Logging
{
    public abstract class LogProfileReader
    {
        public LogProfileReader(string logProfileFilePath, string logPointFilePath)
        {
            LoadFiles(logProfileFilePath, logPointFilePath);
        }

        private static LogProfileReader current;
        public static LogProfileReader Current
        {
            get
            {
                if (current == null) Current = null;
                return current;
            }
            set
            {
                if (value == null)
                    current = new DefaultLogProfileReader("LogProfiles.xml", "LogPoints.xml");
                else
                    current = value;
            }
        }

        private HashSet<string> disabledLogs = null;
        private bool isNeedToBeReload = false;

        public string LogProfileFilePath { get; private set; }


        public string LogPointFilePath { get; private set; }

        protected void LoadFiles(string logProfileFilePath, string logPointFilePath)
        {
            LogProfileFilePath = logProfileFilePath;
            LogPointFilePath = logPointFilePath;
            lock (LogProfileFilePath)
                lock (LogPointFilePath)
                {
                    Reload();
                    AddCacheDependency(LogProfileFilePath, "profile");
                    AddCacheDependency(LogPointFilePath, "logPoint");
                }
        }

        private void AddCacheDependency(string filePath, string key)
        {

            if (System.Web.HttpContext.Current.Cache["__logProfileCacheChange" + key] != null)
                System.Web.HttpContext.Current.Cache.Remove("__logProfileCacheChange" + key);

            isNeedToBeReload = false;

            if (!string.IsNullOrWhiteSpace(filePath))
                System.Web.HttpContext.Current.Cache.Add("__logProfileCacheChange" + key, "x" + key, new CacheDependency(filePath), DateTime.MaxValue, TimeSpan.Zero, CacheItemPriority.High, DependentFilesChanged);
        }

        private void DependentFilesChanged(string key, object value, CacheItemRemovedReason reason)
        {
            //System.Web.HttpContext.Current.Cache.Remove("__logProfileCacheChange");
            isNeedToBeReload = true;
        }

        public void Reload()
        {
            try
            {


                var xmlProfiles = new XmlDocument();
                xmlProfiles.Load(LogProfileFilePath);

                var xmlLogPoints = new XmlDocument();
                xmlLogPoints.Load(LogPointFilePath);

                string activeProfileId = xmlProfiles.DocumentElement.Attributes["ActiveProfileId"].Value;

                var activeProfile = xmlProfiles.SelectSingleNode("/LogProfiles/Profile[@Id='" + activeProfileId + "']");
                if (activeProfile == null) return;

                disabledLogs = new HashSet<string>();

                //DropAllNoLogDefaults means we consider all logpoints in a same way (ignoring logpoint default value)
                if (activeProfile.SelectSingleNode("DropAllNoLogDefaults") == null)
                    foreach (XmlNode node in xmlLogPoints.SelectNodes("LogPoints/Log[@Default='NoLog']"))
                        disabledLogs.Add(node.Attributes["Id"].Value);

                foreach (XmlNode node in activeProfile.ChildNodes)
                {
                    if (node.Name == "ExceptAll")
                    {
                        disabledLogs = null;//null means all are disabled
                        break;
                    }
                    else if (node.Name == "Except")
                        disabledLogs.Add(node.Attributes["LogPoint"].Value);
                    else if (node.Name == "Include")
                    {
                        string value = node.Attributes["LogPoint"].Value;
                        //note : we parse continously. so if we see a include, we remove the previously added "except" if any
                        disabledLogs.Remove(value);
                    }
                }

            }
            catch (Exception x)
            {
                throw new Exception("Exception in parsing [" + LogProfileFilePath + "] or [" + LogPointFilePath + "] file. Check the syntax first to compromise log needs", x);
            }

        }

        public bool IsLogDisabled(string logId)
        {
            if (isNeedToBeReload) LoadFiles(LogProfileFilePath, LogPointFilePath);
            if (disabledLogs == null) return true;//null means all are disabled
            return disabledLogs.Contains(logId);
        }

        public bool IsLogDisabled(int logId, Type logEnumType)
        {
            return IsLogDisabled(Enum.GetName(logEnumType, logId));
        }
    }

}
