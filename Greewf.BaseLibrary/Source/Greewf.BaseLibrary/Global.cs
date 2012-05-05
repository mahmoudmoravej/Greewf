﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Greewf.BaseLibrary
{
    public static class Global
    {
        public const string PHONEREGX = @"\s*0([1-9]{1})([0-9]{1,6})-([0-9]{4,12})\s*$";
        public const string DIGITSREGX = @"([0-9])*";

        static PersianCalendar pcal = new PersianCalendar();
        const string DATEFORMAT = "{0:0000}/{1:00}/{2:00}";
        const string DATETIMEFORMAT = "{0:0000}/{1:00}/{2:00} - {3:00}:{4:00}";
        const string TIMEFORMAT = "{0:00}:{1:00}";

        public static string DisplayDate(DateTime? date)
        {
            if (date.HasValue && date.Value != DateTime.MinValue)
                return string.Format(DATEFORMAT, pcal.GetYear(date.Value), pcal.GetMonth(date.Value), pcal.GetDayOfMonth(date.Value));
            return "";
        }

        public static string DisplayCurrentDate()
        {
            var date = DateTime.Now;
            return string.Format(DATEFORMAT, pcal.GetYear(date), pcal.GetMonth(date), pcal.GetDayOfMonth(date));
        }

        public static string DisplayDateTime(DateTime? date)
        {
            if (date.HasValue && date.Value != DateTime.MinValue)
                return string.Format(DATETIMEFORMAT, pcal.GetYear(date.Value), pcal.GetMonth(date.Value), pcal.GetDayOfMonth(date.Value), date.Value.Hour, date.Value.Minute);
            return "";
        }

        public static string DisplayTime(DateTime? dateTime)
        {
            if (dateTime.HasValue)
                return string.Format(TIMEFORMAT, dateTime.Value.Hour, dateTime.Value.Minute);
            return "";
        }


        public static string DisplayDate(string date)
        {
            if (string.IsNullOrWhiteSpace(date)) return null;//help to use ?? operator
            if (IsValidDate(date))
                return string.Format(DATEFORMAT, date.Substring(0, 4), date.Substring(4, 2), date.Substring(6, 2));
            return date;
        }

        public static string DisplayMonth(string date)
        {
            if (string.IsNullOrWhiteSpace(date)) return null;//help to use ?? operator
            return string.Format("{0}/{1}", date.Substring(0, 4), date.Substring(4, 2));
        }


        public static bool IsValidDate(string date)
        {
            try
            {
                pcal.ToDateTime(
                    int.Parse(date.Substring(0, 4)),
                    int.Parse(date.Substring(4, 2)),
                    int.Parse(date.Substring(6, 2)),
                    0,
                    0,
                    0,
                    0);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static DateTime? ToSystemDateTime(string persianDateTime)
        {
            persianDateTime=persianDateTime.Trim();
            var p = persianDateTime.Replace("-", "/").Replace(":", "/").Replace(" ", "").Split('/').ToList();

            if (p.Count == 1)//means database formated date-time
            {               
                if (persianDateTime.Length == 8)
                {
                    p= new List<string>();
                    p.Add( persianDateTime.Substring(0, 4));
                    p.Add( persianDateTime.Substring(4, 2));
                    p.Add( persianDateTime.Substring(6, 2));                    
                }
                else if (persianDateTime.Length == 12)
                {
                    p = new List<string>();
                    p.Add(persianDateTime.Substring(0, 4));
                    p.Add(persianDateTime.Substring(4, 2));
                    p.Add(persianDateTime.Substring(6, 2));
                    p.Add(persianDateTime.Substring(8, 1));
                    p.Add(persianDateTime.Substring(10, 2));
                }
            }

            if (p.Count == 3)//just date
            {
                p.Add("0");
                p.Add("0");
            }
            else if (p.Count == 2)//just time
            {
                p.Insert(0, DateTime.MinValue.Day.ToString());
                p.Insert(0, DateTime.MinValue.Month.ToString());
                p.Insert(0, DateTime.MinValue.Year.ToString());
            }

            try
            {
                return pcal.ToDateTime(
                    int.Parse(p[0]),
                    int.Parse(p[1]),
                    int.Parse(p[2]),
                    int.Parse(p[3]),
                    int.Parse(p[4]),
                    0,
                    0);
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static string DisplayDateTime(string date)
        {
            if (string.IsNullOrWhiteSpace(date)) return null;//help to use ?? operator
            return string.Format(DATETIMEFORMAT, date.Substring(0, 4), date.Substring(4, 2), date.Substring(6, 2), date.Substring(8, 2), date.Substring(10, 2));
        }



        public static string CurrentDate()
        {
            var pcal = new System.Globalization.PersianCalendar();
            return string.Format("{0:0000}{1:00}{2:00}", pcal.GetYear(DateTime.Now), pcal.GetMonth(DateTime.Now), pcal.GetDayOfMonth(DateTime.Now));
        }

        public static string CurrentDateTime()
        {
            var pcal = new System.Globalization.PersianCalendar();
            return string.Format("{0:0000}{1:00}{2:00}{3:00}{4:00}", pcal.GetYear(DateTime.Now), pcal.GetMonth(DateTime.Now), pcal.GetDayOfMonth(DateTime.Now), DateTime.Now.Hour, DateTime.Now.Minute);
        }

        public static string ToDatabaseFormatDate(string displayDate)
        {
            var p = displayDate.Split('/');
            if (p.Length != 3) return null;
            if (p[0].Length == 2) p[0] = CurrentDate().Substring(0, 2) + p[0];

            var result = string.Format("{0:0000}{1:00}{2:00}", p[0], p[1], p[2]);

            if (IsValidDate(result))
                return result;
            else
                return null;
        }

        public static string ToDatabaseDateTime(DateTime? date)
        {
            if (date.HasValue && date.Value != DateTime.MinValue)
                return string.Format("{0:0000}{1:00}{2:00}{3:00}{4:00}", pcal.GetYear(date.Value), pcal.GetMonth(date.Value), pcal.GetDayOfMonth(date.Value), date.Value.Hour, date.Value.Minute);
            return "";
        }

        public static string ToDatabaseDate(DateTime? date)
        {
            if (date.HasValue && date.Value != DateTime.MinValue)
                return string.Format("{0:0000}{1:00}{2:00}", pcal.GetYear(date.Value), pcal.GetMonth(date.Value), pcal.GetDayOfMonth(date.Value));
            return "";
        }

        public static string NumberToString(float no)
        {
            return NumberToString(no.ToString());
        }

        public static string NumberToString(double no)
        {
            return NumberToString(no.ToString());
        }

        public static string NumberToString(decimal no)
        {
            return NumberToString(no.ToString());
        }

        public static string NumberToString(string no)
        {
            no = no.Replace(",", "");
            no = ((int)Math.Truncate(Convert.ToDecimal(no))).ToString();
            return Number2String.Num2Str(no);
        }

        public static string NumberToString(int no)
        {
            return Number2String.Num2Str(no.ToString());
        }

        private class Number2String
        {
            private static string[] yakan = new string[10] { "صفر", "یک", "دو", "سه", "چهار", "پنج", "شش", "هفت", "هشت", "نه" };
            private static string[] dahgan = new string[10] { "", "", "بیست", "سی", "چهل", "پنجاه", "شصت", "هفتاد", "هشتاد", "نود" };
            private static string[] dahyek = new string[10] { "ده", "یازده", "دوازده", "سیزده", "چهارده", "پانزده", "شانزده", "هفده", "هجده", "نوزده" };
            private static string[] sadgan = new string[10] { "", "یکصد", "دویست", "سیصد", "چهارصد", "پانصد", "ششصد", "هفتصد", "هشتصد", "نهصد" };
            private static string[] basex = new string[5] { "", "هزار", "میلیون", "میلیارد", "تریلیون" };


            private static string getnum3(int num3)
            {
                string s = "";
                int d3, d12;
                d12 = num3 % 100;
                d3 = num3 / 100;
                if (d3 != 0)
                    s = sadgan[d3] + " و ";
                if ((d12 >= 10) && (d12 <= 19))
                {
                    s = s + dahyek[d12 - 10];
                }
                else
                {
                    int d2 = d12 / 10;
                    if (d2 != 0)
                        s = s + dahgan[d2] + " و ";
                    int d1 = d12 % 10;
                    if (d1 != 0)
                        s = s + yakan[d1] + " و ";
                    s = s.Substring(0, s.Length - 3);
                };
                return s;
            }

            public static string Num2Str(string snum)
            {
                string stotal = "";
                if (snum == "0")
                {
                    return yakan[0];
                }
                else
                {
                    snum = snum.PadLeft(((snum.Length - 1) / 3 + 1) * 3, '0');
                    int L = snum.Length / 3 - 1;
                    for (int i = 0; i <= L; i++)
                    {
                        int b = int.Parse(snum.Substring(i * 3, 3));
                        if (b != 0)
                            stotal = stotal + getnum3(b) + " " + basex[L - i] + " و ";
                    }
                    stotal = stotal.Substring(0, stotal.Length - 3);
                }
                return stotal;
            }



        }





    }

}
