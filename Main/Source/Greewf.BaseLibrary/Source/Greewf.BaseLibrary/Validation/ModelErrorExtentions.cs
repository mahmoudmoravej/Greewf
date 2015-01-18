using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;

namespace Greewf.BaseLibrary
{
    public static class ModelErrorExtentions
    {

        public static string GetCode(this ModelError modelError)
        {
            if (modelError is ExtendedModelError) 
                return ((ExtendedModelError)(modelError)).Code;
            return null;
        }

    }

}
