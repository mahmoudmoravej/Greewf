using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;

namespace Greewf.BaseLibrary
{
    public class ExtendedModelError : ModelError
    {
        public string Code { get; set; }

        public ExtendedModelError(string errorMessage, string code = null)
            : base(errorMessage)
        {
            this.Code = code;
        }

        public ExtendedModelError(Exception exception, string code = null)
            : base(exception)
        {
            this.Code = code;
        }
    }
}
