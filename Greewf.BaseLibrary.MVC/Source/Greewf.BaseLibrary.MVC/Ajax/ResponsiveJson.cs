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
        Information=0,
        Success =1,
        Warning=2,
        Faield=3,
    }

    public class ResponsiveJsonResult : JsonResult
    {

        public ResponsiveJsonResult(ResponsiveJsonType type, string message)
        {
            Message = message;
            ResponseType = type;
        }

        private string _message;
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                Data = new { Message, ResponseType };
            }
        }

        private ResponsiveJsonType _responseType;
        public ResponsiveJsonType ResponseType
        {
            get
            {
                return _responseType;
            }
            set
            {
                _responseType = value;
                Data = new { Message, ResponseType };
            }
        }

    }
}
