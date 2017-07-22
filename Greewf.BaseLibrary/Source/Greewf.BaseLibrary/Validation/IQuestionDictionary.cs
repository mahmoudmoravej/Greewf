using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Greewf.BaseLibrary
{
    public interface IQuestionDictionary : IQuestionDictionary<Object>
    {
    }

    public interface IQuestionDictionary<in M>
    {
        void AddQuestion(string key, string questionMessage);
        void AddQuestion(string key, string questionMessage, string code);
        bool HasQuestions();
        string[] Questions { get; }
        void ClearQuestions();

    }

 

}
