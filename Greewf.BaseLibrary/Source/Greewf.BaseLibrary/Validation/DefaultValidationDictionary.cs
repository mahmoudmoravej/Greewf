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
        List<KeyValuePair<string, ExtendedModelError>> _lstQuestions = new List<KeyValuePair<string, ExtendedModelError>>();

        #region Errors

        public void AddError(string key, string errorMessage)
        {
            _lstErrors.Add(new KeyValuePair<string, ExtendedModelError>(key, new ExtendedModelError(errorMessage)));
        }

        public void AddError(string key, string errorMessage, string code)
        {
            _lstErrors.Add(new KeyValuePair<string, ExtendedModelError>(key, new ExtendedModelError(errorMessage, code)));
        }

        public string[] Errors
        {
            get
            {
                return _lstErrors.Select(o => o.Value.ErrorMessage).ToArray<string>();

            }
        }
        #endregion

        #region Warnings

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

        #endregion

        #region Questions

        public void AddQuestion(string key, string questionMessage)
        {
            _lstQuestions.Add(new KeyValuePair<string, ExtendedModelError>(key, new ExtendedModelError(questionMessage)));
        }

        public void AddQuestion(string key, string questionMessage, string code)
        {
            _lstQuestions.Add(new KeyValuePair<string, ExtendedModelError>(key, new ExtendedModelError(questionMessage, code)));
        }

        public bool HasQuestions()
        {
            return _lstQuestions.Count > 0;
        }

        public string[] Questions
        {
            get
            {
                return _lstQuestions.Select(o => o.Value.ErrorMessage).ToArray<string>();

            }
        }

        public void ClearQuestions()
        {
            _lstQuestions.Clear();
        }

        #endregion



        public bool IsValid
        {
            get
            {
                return _lstErrors.Count == 0 && _lstQuestions.Count == 0;
            }
        }

        public void Clear()
        {
            _lstErrors.Clear();
            _lstWarnings.Clear();
            _lstQuestions.Clear();
        }



    }
}
