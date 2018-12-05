

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Net;
using Greewf.BaseLibrary.IP;

namespace Greewf.BaseLibrary
{
    /// <summary>
    /// بدلیل آنکه بسیاری از توابع را در گزارش هم نیاز داشتیم و آنجا مهم بود که اسمبلی بسیار ساده باشد رفرنس بسیاری از توابع را برای
    /// جلوگیری از تکرار به آنجا داده ایم
    /// </summary>
    public static class Global
    {
        //inlined one-pixel transparent GIF :
        public const string INLINEIMAGEPLACEHOLDER = "data:image/gif;base64,R0lGODlhAQABAIABAP///wAAACH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==";

        public const string PHONEREGX = @"\s*0([1-9]{1})([0-9]{1,6})-([0-9]{4,12})\s*$";
        public const string DIGITSREGX = @"([0-9])*";
        public const string PERSIANCHARACTERSREGX = @"^[\u0600-\u06FF\uFB8A\u067E\u0686\u06AF\u200C\u200F ]+$";//check this http://stackoverflow.com/a/34869397/790811

        static PersianCalendar pcal = new PersianCalendar();
        const string DATEFORMAT = "{0:0000}/{1:00}/{2:00}";
        const string DATETIMEFORMAT = "{0:0000}/{1:00}/{2:00} - {3:00}:{4:00}";
        const string FULLDATETIMEFORMAT = "{0:0000}/{1:00}/{2:00} - {3:00}:{4:00}:{5:00}{6}";
        const string TIMEFORMAT = "{0:00}:{1:00}";

        public static string DisplayDate(DateTime? date)
        {
            return Reporting.Global.DisplayDate(date);
        }

        public static string DisplayCurrentDate()
        {
            return Reporting.Global.DisplayCurrentDate();
        }

        public static string DisplayDateTime(DateTime? date)
        {
            return Reporting.Global.DisplayDateTime(date);
        }

        public static string DisplayFullDateTime(DateTime? date, bool includeMilliSecond = false)
        {
            return Reporting.Global.DisplayFullDateTime(date, includeMilliSecond);
        }

        public static string DisplayTime(DateTime? dateTime)
        {
            return Reporting.Global.DisplayTime(dateTime);
        }


        public static string DisplayDate(string date)
        {
            return Reporting.Global.DisplayDate(date);
        }

        public static string DisplayMonth(string date)
        {
            return Reporting.Global.DisplayMonth(date);
        }


        public static bool IsValidDate(string date)
        {
            return Reporting.Global.IsValidDate(date);
        }

        public static DateTime? ToSystemDateTime(string persianDateTime)
        {
            return Reporting.Global.ToSystemDateTime(persianDateTime);
        }


        public static string DisplayDateTime(string date)
        {
            return Reporting.Global.DisplayDateTime(date);
        }



        public static string CurrentDate()
        {
            return Reporting.Global.CurrentDate();
        }

        public static string CurrentDateTime()
        {
            return Reporting.Global.CurrentDateTime();
        }

        public static string ToDatabaseFormatDate(string displayDate)
        {
            return Reporting.Global.ToDatabaseFormatDate(displayDate);
        }

        public static string ToDatabaseDateTime(DateTime? date)
        {
            return Reporting.Global.ToDatabaseDateTime(date);
        }

        public static string ToDatabaseDate(DateTime? date)
        {
            return Reporting.Global.ToDatabaseDate(date);
        }

        public static string NumberToString(float no)
        {
            return Reporting.Global.NumberToString(no);
        }

        public static string NumberToString(double no)
        {
            return Reporting.Global.NumberToString(no);
        }

        public static string NumberToString(decimal no)
        {
            return Reporting.Global.NumberToString(no);
        }

        public static string NumberToString(string no)
        {
            return Reporting.Global.NumberToString(no);
        }

        public static string NumberToString(int no)
        {
            return Reporting.Global.NumberToString(no);
        }

        public static string NumberToOrdinalString(int no)
        {
            return Reporting.Global.NumberToOrdinalString(no);
        }

        public static string NumberToOrdinalString2(int no)
        {
            return Reporting.Global.NumberToOrdinalString2(no);
        }


        public static object FormatPersianNumber(object value)
        {
            return Reporting.Global.FormatPersianNumber(value);
        }

        public static object FormatPersianNumber(object value, string format)
        {
            return Reporting.Global.FormatPersianNumber(value, format);
        }

         public static string CorrectPersian(string text)
        {
            return Reporting.Global.CorrectPersian(text);
        }

        public static bool IsNumericType(Type type)
        {
            return Reporting.Global.IsNumericType(type);
        }

        public static string ShowDiffsAsHtml(string oldText, string newText, int? sizeThreshold = null, bool cleanupSemantic = false)
        {
            var dmp = new DiffMatchPatch.diff_match_patch();
            var diffs = dmp.diff_main(oldText ?? "", newText ?? "");

            if (sizeThreshold != null &&
                ((oldText != null && oldText.Length <= sizeThreshold) ||
                 (newText != null && newText.Length <= sizeThreshold)
                )
               )
                return
                    (string.IsNullOrEmpty(oldText) ? "" : "<del style=\"background:#ffe6e6;display:inline-block;padding:4px 8px 4px 8px;\">" + CorrectHtml(oldText) + "</del>" + "&nbsp;") +
                    (string.IsNullOrEmpty(newText) ? "" : "<ins style=\"background:#e6ffe6;display:inline-block;padding:4px 8px 4px 8px;\">" + CorrectHtml(newText) + "</ins>");

            if (cleanupSemantic)
                dmp.diff_cleanupSemantic(diffs);

            return dmp.diff_prettyHtml(diffs);

        }

        private static string CorrectHtml(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            return str//comes from diff_prettyHtml function
                 .Replace("&", "&amp;")
                 .Replace("<", "&lt;")
                 .Replace(">", "&gt;")
                 .Replace("\n", "&para;<br>");
        }

        public static bool IsIpInRange(string ip, string lowerBandIp, string upperBandIp)
        {
            var range = new IpAddressRange(lowerBandIp, upperBandIp);
            return range.IsInRange(ip);
        }

        public static bool IsIpInRange(IPAddress ip, IPAddress lowerBandIp, IPAddress upperBandIp)
        {
            var range = new IpAddressRange(lowerBandIp, upperBandIp);
            return range.IsInRange(ip);
        }


        public static bool? IsInternetIp(string ip, bool nullOnException = false)
        {
            try
            {
                return IpAddressRange.IsInternetIp(ip);
            }
            catch (Exception x)
            {
                if (nullOnException)
                    return null;
                else
                    throw x;
            }
        }

        public static bool IsInternetIp(IPAddress ip)
        {
            return IpAddressRange.IsInternetIp(ip);
        }

        public static bool IsLoopbackIp(string ip)
        {
            return IpAddressRange.IsLoopbackIp(ip);
        }

        public static bool IsLoopbackIp(IPAddress ip)
        {
            return IpAddressRange.IsLoopbackIp(ip);
        }

    }

}

