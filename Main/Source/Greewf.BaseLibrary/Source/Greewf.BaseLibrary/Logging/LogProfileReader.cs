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

        public Func<string> ProfileIdRetreiver { private get; set; }

        private HashSet<string> _disabledLogs = null;
        private bool _isNeedToBeReload = false;
        private string _overridedProfileId = null;//if it has value, it means that we should not use ActiveProfileId of LogProfiles.xml in automatic reoloading (i.e. when the DependentFilesChanged called)

        public string LogProfileFilePath { get; private set; }


        public string LogPointFilePath { get; private set; }

        protected void LoadFiles(string logProfileFilePath, string logPointFilePath)
        {
            LogProfileFilePath = logProfileFilePath;
            LogPointFilePath = logPointFilePath;
            lock (LogProfileFilePath)
                lock (LogPointFilePath)
                {
                    LoadProfile();
                    AddCacheDependency(LogProfileFilePath, "profile");
                    AddCacheDependency(LogPointFilePath, "logPoint");
                }
        }

        private void AddCacheDependency(string filePath, string key)
        {

            if (System.Web.HttpContext.Current.Cache["__logProfileCacheChange" + key] != null)
                System.Web.HttpContext.Current.Cache.Remove("__logProfileCacheChange" + key);

            _isNeedToBeReload = false;

            if (!string.IsNullOrWhiteSpace(filePath))
                System.Web.HttpContext.Current.Cache.Add("__logProfileCacheChange" + key, "x" + key, new CacheDependency(filePath), DateTime.MaxValue, TimeSpan.Zero, CacheItemPriority.High, DependentFilesChanged);
        }

        private void DependentFilesChanged(string key, object value, CacheItemRemovedReason reason)
        {
            //System.Web.HttpContext.Current.Cache.Remove("__logProfileCacheChange");
            _isNeedToBeReload = true;
        }

        public void LoadProfile(string profileId = null)
        {
            try
            {
                var xmlProfiles = new XmlDocument();
                xmlProfiles.Load(LogProfileFilePath);

                var xmlLogPoints = new XmlDocument();
                xmlLogPoints.Load(LogPointFilePath);

                if (profileId != null)
                    _overridedProfileId = profileId;
                else
                    profileId = _overridedProfileId;

                profileId = profileId ?? xmlProfiles.DocumentElement.Attributes["ActiveProfileId"].Value;

                var activeProfile = xmlProfiles.SelectSingleNode("/LogProfiles/Profile[@Id='" + profileId + "']");
                if (activeProfile == null) return;

                var enabledLogs = new HashSet<string>();

                foreach (XmlNode node in xmlLogPoints.SelectNodes("LogPoints/Log[not(@Default)]|LogPoints/Log[@Default!='NoLog']"))
                    enabledLogs.Add(node.Attributes["Id"].Value);


                foreach (XmlNode node in activeProfile.ChildNodes)
                {
                    if (node.Name == "AddAllNoLogDefaults")
                    {
                        var arr = new List<string>();
                        foreach (XmlNode n in xmlLogPoints.SelectNodes("LogPoints/Log[@Default='NoLog']"))
                            arr.Add(n.Attributes["Id"].Value);

                        enabledLogs = new HashSet<string>(enabledLogs.Union(arr));

                    }
                    if (node.Name == "ExceptAll")
                    {
                        enabledLogs = null;//null means all are disabled
                        break;
                    }
                    else if (node.Name == "ClearAll")
                    {
                        enabledLogs.Clear();//null means all are disabled                        
                    }
                    else if ((node.Name == "Except" || node.Name == "Remove") && node.Attributes["LogPoint"] != null)
                        enabledLogs.Remove(node.Attributes["LogPoint"].Value);
                    else if (node.Name == "Remove" && node.Attributes["Group"] != null)
                    {
                        var arr = new List<string>();
                        string groupName = node.Attributes["Group"].Value;
                        foreach (XmlNode n in xmlLogPoints.SelectNodes(string.Format("LogPoints/Log[@Group='{0}']", groupName)))
                            arr.Add(n.Attributes["Id"].Value);

                        enabledLogs = new HashSet<string>(enabledLogs.Except(arr));
                    }
                    else if ((node.Name == "Include" || node.Name == "Add") && node.Attributes["LogPoint"] != null)
                    {
                        string value = node.Attributes["LogPoint"].Value;
                        //note : we parse continously. so if we see a include, we remove the previously added "except" if any
                        enabledLogs.Add(value);
                    }
                    else if ( node.Name == "Add" && node.Attributes["Group"] != null)
                    {
                        var arr = new List<string>();
                        string groupName = node.Attributes["Group"].Value;
                        foreach (XmlNode n in xmlLogPoints.SelectNodes(string.Format("LogPoints/Log[not(@Default)][@Group='{0}']|LogPoints/Log[@Default!='NoLog'][@Group='{0}']", groupName)))
                            arr.Add(n.Attributes["Id"].Value);

                        enabledLogs = new HashSet<string>(enabledLogs.Union(arr));
                    }
                }

                if (enabledLogs == null || enabledLogs.Count == 0)
                    _disabledLogs = null;
                else
                {
                    var allLogs = new List<string>();
                    foreach (XmlNode node in xmlLogPoints.SelectNodes("LogPoints/Log"))
                        allLogs.Add(node.Attributes["Id"].Value);

                    _disabledLogs = new HashSet<string>(allLogs.Except(enabledLogs).Distinct());
                }


            }
            catch (Exception x)
            {
                throw new Exception("Exception in parsing [" + LogProfileFilePath + "] or [" + LogPointFilePath + "] file. Check the syntax first to compromise log needs", x);
            }

        }

        public Dictionary<string, string> GetProfiles(string attributeName = null, string attributeValue = null)
        {
            var xmlProfiles = new XmlDocument();
            var query = "LogProfiles/Profile";
            var result = new Dictionary<string, string>();

            query = string.IsNullOrEmpty(attributeName) ? query : query + string.Format("[@{0}='{1}']", attributeName, attributeValue);
            xmlProfiles.Load(LogProfileFilePath);

            foreach (XmlNode profile in xmlProfiles.SelectNodes(query))
                result.Add(profile.Attributes["Id"].Value, profile.Attributes["Title"].Value);

            return result;
        }

        public bool IsLogDisabled(string logId)
        {
            if (ProfileIdRetreiver != null) RetriveProfileId();
            if (_isNeedToBeReload) LoadFiles(LogProfileFilePath, LogPointFilePath);

            if (_disabledLogs == null) return true;//null means all are disabled
            return _disabledLogs.Contains(logId);
        }

        private void RetriveProfileId()
        {
            var retreivedId = ProfileIdRetreiver();
            if (retreivedId != _overridedProfileId)
            {
                _overridedProfileId = retreivedId;//we need it to reset to default if nothing is passed.
                LoadProfile();
            }

        }

        public bool IsLogDisabled(int logId, Type logEnumType)
        {
            return IsLogDisabled(Enum.GetName(logEnumType, logId));
        }
    }

}
