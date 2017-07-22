using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Greewf.BaseLibrary
{
    public interface IWarningDictionary : IWarningDictionary<Object>
    {
    }

    public interface IWarningDictionary<in M>
    {
        void AddWarning(string key, string warningMessage);
        void AddWarning(string key, string warningMessage, string code);
        bool HasWarnings();
        string[] Warnings { get; }
        void ClearWarnings();

    }

 

}
