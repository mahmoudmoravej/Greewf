using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Greewf.BaseLibrary.Logging
{
    public partial class Log
    {
        static Regex _unicodeReplacer = new Regex(@"\\[uU]([0-9A-Fa-f]{4})"); //http://stackoverflow.com/questions/183907/how-do-convert-unicode-escape-sequences-to-unicode-characters-in-a-net-string

        private string _requestBody;

        
        //NOTE!!! : if you update edmx model, you should remove the same property from Log.cs file after code-generation.
        public string RequestBody
        {
            get
            {
                return _requestBody;
            }
            set
            {
                //because in some cases it sends some string like this : 'NewPassword':'\u0633\u0644\u0627\u0645\u062a\u06cc'
                _requestBody = string.IsNullOrEmpty(value) ? null : _unicodeReplacer.Replace(value, match => ((char)Int32.Parse(match.Value.Substring(2), NumberStyles.HexNumber)).ToString());
            }
        }

    }
}
