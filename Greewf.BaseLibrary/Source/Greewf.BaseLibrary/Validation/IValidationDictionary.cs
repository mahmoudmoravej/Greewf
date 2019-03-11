namespace Greewf.BaseLibrary
{
    public interface IValidationDictionary : IValidationDictionary<object>
    {
    }

    public interface IValidationDictionary<in M> : IWarningDictionary<object>, IQuestionDictionary<object>
    {
        void AddError(string key, string errorMessage);
        void AddError(string key, string errorMessage, string code);
        bool IsValid { get; }
        string[] Errors { get; }
        void Clear();

    }




}
