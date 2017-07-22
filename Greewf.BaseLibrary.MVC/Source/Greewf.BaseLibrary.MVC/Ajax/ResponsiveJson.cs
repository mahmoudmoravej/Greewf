using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc.Ajax;
using System.Web.Mvc;

namespace Greewf.BaseLibrary.MVC.Ajax
{
    public enum ResponsiveJsonType
    {
        Information = 0,
        Success = 1,
        Warning = 2,
        Faield = 3,
    }

    public class ResponsiveJsonResult : JsonResult
    {

        internal ResponsiveJsonResult()
        {
            JsonRequestBehavior = JsonRequestBehavior.AllowGet;
        }

        internal ResponsiveJsonResult(ResponsiveJsonType type, string message)
            : this()
        {
            Message = message;
            ResponseType = type;

        }

        internal ResponsiveJsonResult(ResponsiveJsonType type, string message, string details)
            : this(type, message)
        {
            Details = details;
        }

        internal ResponsiveJsonResult(ResponsiveJsonType type, ModelStateDictionary modelState)
        {
            string result = "<ul>";
            JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            ResponseType = type;
            int count = 0;

            foreach (var item in modelState)
            {
                foreach (var err in item.Value.Errors)
                {
                    if (!string.IsNullOrEmpty(err.ErrorMessage))
                    {
                        result += "<li>" + err.ErrorMessage + "</li>";
                        count++;
                    }
                    if (err.Exception != null)
                    {
                        result += "<li>" + err.Exception.ToString() + "</li>";
                        count++;
                    }
                }
            }

            result += "</ul>";
            if (count > 0)
                result = result.Replace("<li>", "").Replace("</li>", "").Replace("<ul>", "").Replace("</ul>", "");

            Message = result;

        }

        internal ResponsiveJsonResult(ResponsiveJsonType type, ModelStateDictionary modelState, string details)
            : this(type, modelState)
        {
            Details = details;
        }
        internal ResponsiveJsonResult(ModelStateDictionary modelState)
            : this(ResponsiveJsonType.Faield, modelState)
        {
        }

        internal ResponsiveJsonResult(ModelStateDictionary modelState, string details)
            : this(ResponsiveJsonType.Faield, modelState, details)
        {
        }


        public override void ExecuteResult(ControllerContext context)
        {
            base.ExecuteResult(context);
        }

        private ResponsiveJsonType _responseType;
        private string _message;
        private string _details = null;

        public ResponsiveJsonType ResponseType
        {
            get
            {
                return _responseType;
            }
            set
            {
                _responseType = value;
                Data = new { Message, ResponseType, Details };
            }
        }
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                Data = new { Message, ResponseType, Details };
            }
        }
        public string Details
        {
            get
            {
                return _details;
            }
            set
            {
                _details = value;
                Data = new { Message, ResponseType, Details };
            }
        }

    }
}
