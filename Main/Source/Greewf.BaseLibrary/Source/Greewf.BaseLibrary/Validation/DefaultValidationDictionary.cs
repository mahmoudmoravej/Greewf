using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greewf.BaseLibrary
{

    public class DefaultValidationDictionary : IValidationDictionary
    {

        List<KeyValuePair<string, string>> _lstErrors = new List<KeyValuePair<string, string>>();
        List<KeyValuePair<string, string>> _lstWarnings = new List<KeyValuePair<string, string>>();

        public void AddError(string key, string errorMessage)
        {
            _lstErrors.Add(new KeyValuePair<string, string>(key, errorMessage));
        }

        public bool IsValid
        {
            get
            {
                return _lstErrors.Count == 0;
            }
        }

        public string[] Errors
        {
            get
            {
                return _lstErrors.Select(o => o.Value).ToArray<string>();

            }
        }


        public void AddWarning(string key, string warningMessage)
        {
            _lstWarnings.Add(new KeyValuePair<string, string>(key, warningMessage));
        }

        public bool HasWarnings()
        {
            return _lstWarnings.Count > 0;
        }

        public string[] Warnings
        {
            get
            {
                return _lstWarnings.Select(o => o.Value).ToArray<string>();

            }
        }

        public void ClearWarnings()
        {
            _lstWarnings.Clear();
        }


        public void Clear()
        {
            _lstErrors.Clear();
            _lstWarnings.Clear();
        }
    }
}
