using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greewf.BaseLibrary
{

    public class DefaultValidationDictionary : IValidationDictionary
    {

        List<KeyValuePair<string, ExtendedModelError>> _lstErrors = new List<KeyValuePair<string, ExtendedModelError>>();
        List<KeyValuePair<string, ExtendedModelError>> _lstWarnings = new List<KeyValuePair<string, ExtendedModelError>>();

        public void AddError(string key, string errorMessage)
        {
            _lstErrors.Add(new KeyValuePair<string, ExtendedModelError>(key, new ExtendedModelError(errorMessage)));
        }

        public void AddError(string key, string errorMessage, string code)
        {
            _lstErrors.Add(new KeyValuePair<string, ExtendedModelError>(key, new ExtendedModelError(errorMessage, code)));
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
                return _lstErrors.Select(o => o.Value.ErrorMessage).ToArray<string>();

            }
        }


        public void AddWarning(string key, string warningMessage)
        {
            _lstWarnings.Add(new KeyValuePair<string, ExtendedModelError>(key, new ExtendedModelError(warningMessage)));
        }

        public void AddWarning(string key, string warningMessage, string code)
        {
            _lstWarnings.Add(new KeyValuePair<string, ExtendedModelError>(key, new ExtendedModelError(warningMessage, code)));
        }

        public bool HasWarnings()
        {
            return _lstWarnings.Count > 0;
        }

        public string[] Warnings
        {
            get
            {
                return _lstWarnings.Select(o => o.Value.ErrorMessage).ToArray<string>();

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
