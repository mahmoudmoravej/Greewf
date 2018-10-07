using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Text.RegularExpressions;

namespace effts
{
    /// <summary>
    /// 
    /// </summary>
    public class FtsInterceptor : IDbCommandInterceptor
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            RewriteFullTextQuery(command);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            RewriteFullTextQuery(command);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="interceptionContext"></param>
        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        public static void RewriteFullTextQuery(DbCommand cmd)
        {
            /*
             * توجه : 
             *  --- 1 -----
             *  فرض است که دستورات توسط انتیتی فریم وورک ایجاد شده است
             *  --- 2  ----
             * (?:(\s*?)ESCAPE N'~')?
             * به معنی اسکیپ کاراکتر در دستور لایک است https://docs.microsoft.com/en-us/sql/t-sql/language-elements/like-transact-sql?view=sql-server-2017#pattern-matching-with-the-escape-clause
             * در رجیکس های وارد شده فرض شده است که این اسکیپ با حداکثر یک فضای خالی در رشته وجود دارد
             * بنابراین اگرچه وجود دو فاصله یا بیشتر هم در اس.کیو.ال مشکلی ایجاد نمی کند
             * ولی اینجا برای سادگی رجیکس بدین شکل نوشته شده است
             * و ازآنجا که دستورات با انتیتی فریم وورک ایجاد شده است این موضوع مشکلی ایجاد نمی کند
             * --- 3 ----
             * امکان جستجوی برخی کاراکترهای خاص در فول تکست نیست ولی در کل مشکلی هم ایجاد نمی کند
             */
            var text = cmd.CommandText;
            for (var i = 0; i < cmd.Parameters.Count; i++)
            {
                var parameter = cmd.Parameters[i];
                if (
                    !parameter.DbType.In(DbType.String, DbType.AnsiString, DbType.StringFixedLength,
                        DbType.AnsiStringFixedLength)) continue;
                if (parameter.Value == DBNull.Value)
                    continue;
                var value = (string)parameter.Value;
                if (value.IndexOf(FullTextPrefixes.ContainsPrefix, StringComparison.Ordinal) >= 0)
                {
                    parameter.Size = 4000;
                    parameter.DbType = DbType.String;
                    value = value.Replace(FullTextPrefixes.ContainsPrefix, ""); // remove prefix we added n linq query
                    value = value.Substring(1, value.Length - 2); // remove %% escaping by linq translator from string.Contains to sql LIKE
                    parameter.Value = value;
                    cmd.CommandText = Regex.Replace(text,
                        string.Format(
                            @"\[(\w*)\].\[(\w*)\]\s*LIKE\s*@{0}(?:(\s*?)ESCAPE N'~')?", parameter.ParameterName),
                        string.Format(@"CONTAINS([$1].[$2], @{0})", parameter.ParameterName));
                    if (text == cmd.CommandText)
                        throw new Exception("FTS was not replaced on: " + text);
                    text = cmd.CommandText;
                }
                else if (value.IndexOf(FullTextPrefixes.FreetextPrefix, StringComparison.Ordinal) >= 0)
                {
                    parameter.Size = 4000;
                    parameter.DbType = DbType.String;
                    value = value.Replace(FullTextPrefixes.FreetextPrefix, ""); // remove prefix we added n linq query
                    value = value.Substring(1, value.Length - 2); // remove %% escaping by linq translator from string.Contains to sql LIKE
                    parameter.Value = value;
                    cmd.CommandText = Regex.Replace(text,
                        string.Format(
                            @"\[(\w*)\].\[(\w*)\]\s*LIKE\s*@{0}(?:(\s*?)ESCAPE N'~')?", parameter.ParameterName),
                        string.Format(@"FREETEXT([$1].[$2], @{0})", parameter.ParameterName));
                    if (text == cmd.CommandText)
                        throw new Exception("FTS was not replaced on: " + text);
                    text = cmd.CommandText;
                }
            }

            cmd.CommandText = text;

            //اگر مقدار بصورت پارامتری ارسال نشده بود و در متن کوییری بود. مانند حالتیکه کویری را با کویری ساز مخصوص گریدها ساخته ایم
            //این بخش از کد زیر فقط بخاطر کویری ساز مربوط به گریدها نوشته شده است. اگر می توان آنر بصورت ماجول جدا به کد تزریق کرد بهتر می باشد
            if (text.IndexOf(FullTextPrefixes.ContainsPrefix) >= 0 || text.IndexOf(FullTextPrefixes.FreetextPrefix) >= 0)
            {
                cmd.CommandText = Regex.Replace(cmd.CommandText,
                    string.Format(
                        @"\[(\w*)\].\[(\w*)\]\s*(LIKE\s*N\'%\(-({0}|{1})-(.*?)\)%\')(?:(\s*?)ESCAPE N'~')?", FullTextPrefixes.ContainsFunctionName, FullTextPrefixes.FreeTextFunctionName),
                        "$4([$1].[$2], N'\"*$5*\"')");//ستاره ها برای وایلد جستجو کردن فولتکست می باشد که در ابتدا و انتهای کلمه کلیدی اورده شده است
            }

        }
    }
}
