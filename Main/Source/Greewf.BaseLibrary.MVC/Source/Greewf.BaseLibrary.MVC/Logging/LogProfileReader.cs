using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Greewf.BaseLibrary.MVC.Logging
{
    public abstract class LogProfileReader
    {
        private static LogProfileReader current = new DefaultLogProfileReader();
        public static LogProfileReader Current
        {
            get
            {
                return current;
            }
            set
            {
                if (value == null)
                    current = new DefaultLogProfileReader();
                else
                    current = value;
            }
        }

        private HashSet<string> disabledLogs;

        private string logProfileFilePath;
        public string LogProfileFilePath
        {
            get
            {
                return logProfileFilePath;
            }
            set
            {
                logProfileFilePath = value;
                lock (logProfileFilePath)
                {
                    Reload();
                }
            }
        }

        public void Reload()
        {
            try
            {

                var xmlProfiles = new XmlDocument();
                xmlProfiles.Load(logProfileFilePath);

                string activeProfileId = xmlProfiles.DocumentElement.Attributes["ActiveProfileId"].Value;

                var activeProfile = xmlProfiles.SelectSingleNode("/LogProfiles/Profile[@Id='" + activeProfileId + "']");
                if (activeProfile == null) return;

                disabledLogs = new HashSet<string>();
                foreach (XmlNode node in activeProfile.ChildNodes)
                {
                    if (node.Name == "ExceptAll")
                    {
                        disabledLogs = null;//null means all are disabled
                        break;
                    }
                    else if (node.Name == "Except")
                        disabledLogs.Add(node.Attributes["LogPoint"].Value);
                }
            }
            catch (Exception x)
            {
                throw new Exception("Exception in parsing [" + logProfileFilePath + "] file. Check the syntax first to compromise log needs", x);
            }

        }

        public bool IsLogDisabled(string logId)
        {
            if (disabledLogs == null) return true;//null means all are disabled
            return disabledLogs.Contains(logId);
        }

        public bool IsLogDisabled(int logId, Type logEnumType)
        {
            return IsLogDisabled(Enum.GetName(logEnumType, logId));
        }
    }

    public class DefaultLogProfileReader : LogProfileReader
    {

    }
}
