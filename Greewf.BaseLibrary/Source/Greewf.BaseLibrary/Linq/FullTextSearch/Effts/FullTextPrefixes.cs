namespace effts
{
    /// <summary>
    /// 
    /// </summary>
    public static class FullTextPrefixes
    {
        /*
         NOTE!!
         کلاس 
         FtsInterceptor
         به شکل پرفیکس این توابع بسیار وابسته هست. در صورت تغییر در رجیکس آنجا هم باید تغییر دهید
         */

        public const string ContainsFunctionName = "CONTAINS";
        public const string FreeTextFunctionName = "FREETEXT";
        /// <summary>
        /// 
        /// </summary>
        public const string ContainsPrefix = "-" + ContainsFunctionName + "-";//توجه! متن بین خطوط حتما باید نام تابع باشد

        /// <summary>
        /// 
        /// </summary>
        public const string FreetextPrefix = "-" + FreeTextFunctionName + "-";//توجه! متن بین خطوط حتما باید نام تابع باشد

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="escapeFullTextSpecialCharacters"></param>
        /// <returns></returns>
        public static string Contains(string searchTerm, bool escapeFullTextSpecialCharacters)
        {
            if (escapeFullTextSpecialCharacters)//کاراکتر کوتیشین در فول تکست قابل تشخیص نیست و باید دابل شود
                return string.Format("({0}{1})", ContainsPrefix, searchTerm?.Replace("\"", "\"\""));
            else
                return string.Format("({0}{1})", ContainsPrefix, searchTerm);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchTerm"></param>
        /// /// <param name="escapeFullTextSpecialCharacters"></param>
        /// <returns></returns>
        public static string Freetext(string searchTerm, bool escapeFullTextSpecialCharacters)
        {
            if (escapeFullTextSpecialCharacters)//کاراکتر کوتیشین در فول تکست قابل تشخیص نیست و باید دابل شود
                return string.Format("({0}{1})", FreetextPrefix, searchTerm?.Replace("\"", "\"\"\""));
            else
                return string.Format("({0}{1})", FreetextPrefix, searchTerm);
        }

    }
}
