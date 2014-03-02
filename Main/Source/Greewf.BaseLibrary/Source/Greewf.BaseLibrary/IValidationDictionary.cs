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

    public interface IValidationDictionary<in M>
    {
        void AddError(string key, string errorMessage);
        bool IsValid { get; }
        string[] Errors { get; }
        void Clear();
    }

    public static class ValidationExtentions
    {
        public static string GetName<m, p>(Expression<Func<m, p>> exp)
        {
            MemberExpression body = exp.Body as MemberExpression;

            if (body == null)
            {
                UnaryExpression ubody = (UnaryExpression)exp.Body;
                body = ubody.Operand as MemberExpression;
            }

            return body.Member.Name;
        }

        public static void AddError<M, P>(this IValidationDictionary<M> v, Expression<Func<M, P>> exp, string errorMessage)
        {
            (v as IValidationDictionary).AddError(GetName(exp), errorMessage);
        }
    }


    public class DefaultValidationDictionary : IValidationDictionary
    {

        List<KeyValuePair<string, string>> _lst = new List<KeyValuePair<string, string>>();

        public void AddError(string key, string errorMessage)
        {
            _lst.Add(new KeyValuePair<string, string>(key, errorMessage));
        }

        public bool IsValid
        {
            get
            {
                return _lst.Count == 0;
            }
        }

        public string[] Errors
        {
            get
            {
                return _lst.Select(o => o.Value).ToArray<string>();

            }
        }


        public void Clear()
        {
            _lst.Clear();
        }
    }

}
