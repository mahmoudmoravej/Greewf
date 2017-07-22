using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Greewf.BaseLibrary
{
    public interface IValidationDictionary : IValidationDictionary<Object>
    {
    }

    public interface IValidationDictionary<in M> : IWarningDictionary<Object>, IQuestionDictionary<Object>
    {
        void AddError(string key, string errorMessage);
        void AddError(string key, string errorMessage, string code);
        bool IsValid { get; }
        string[] Errors { get; }
        void Clear();

    }

   


}
