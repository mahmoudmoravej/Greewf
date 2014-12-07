using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections.Specialized;
using System.Reflection;
//TODO : 
//اگر کاربر از کوری استرینگ را مستقیم از آدرس بخواند چطور؟ احتمالا مقادیر تصحیح نشده را بخواند
//

namespace Greewf.BaseLibrary.MVC.HttpModules
{
    public class PersianCharacterCorrecterModule : IHttpModule
    {

        //get the property
        PropertyInfo ReadOnlyPropertyInfo = typeof(NameValueCollection).GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);

        public PersianCharacterCorrecterModule()
        {
        }

        public String ModuleName
        {
            get { return "PersianCharacterCorrecterModule"; }
        }

        // In the Init function, register for HttpApplication 
        // events by adding your handlers.
        public void Init(HttpApplication application)
        {
            application.BeginRequest +=
                (new EventHandler(this.Application_BeginRequest));
        }

        private void Application_BeginRequest(Object source,
             EventArgs e)
        {
            //form
            NameValueCollection formData = HttpContext.Current.Request.Form;

            CorrectCollection(HttpContext.Current.Request.Form);
            CorrectCollection(HttpContext.Current.Request.QueryString);

        }

        //توجه :
        //در یک حالت عجیب که یک فیلد چک باکس بود
        //و ریپلیس آن عملا تغییری در آن ایجاد نمیکرد
        //باعث خطا می شد. لذا با چک کردن این موضوع قبل از ست شدن مقدار خطا برطرف شد
        // اما مشخص نگردید که سورس خطا کجاست و به چه علت است
        private void CorrectCollection(NameValueCollection data)
        {
            if (data.Count > 0)
            {

                //unset readonly
                ReadOnlyPropertyInfo.SetValue(data, false, null);

                foreach (var key in data.AllKeys)
                {
                    string value = data[key];
                    if (IsCorrectionNeeded(value))
                        data[key] = CorrectPersian(value);
                }

                //set readonly
                ReadOnlyPropertyInfo.SetValue(data, true, null);

            }
        }

        public static bool IsCorrectionNeeded(string text)
        {
            return
                text.Contains("ي") ||
                text.Contains("ك") ||
                text.Contains("\\u064a") ||//ي
                text.Contains("\\u0643") || //ك
                text.Contains("%5Cu064a") ||//ي
                text.Contains("%5Cu0643");  //ك
        }

        public static string CorrectPersian(string text)
        {
            return text
                .Replace("ي", "ی")
                .Replace("ك", "ک")
                .Replace("\\u064a", "\\u06cc") //ي to ی     //in body
                .Replace("\\u0643", "\\u06a9") //ك to ک     //in body
                .Replace("%5Cu064a", "%5Cu06cc")//ي to ی    //in uri
                .Replace("%5Cu0643", "%5Cu06a9"); //ك to ک  //in uri
        }

        public void Dispose() { }
    }
}
