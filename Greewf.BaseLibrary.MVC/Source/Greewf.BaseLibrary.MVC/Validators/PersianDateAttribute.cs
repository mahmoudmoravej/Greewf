/*
 * BASED ON THIS ARTICLE : http://haacked.com/archive/2009/11/19/aspnetmvc2-custom-validation.aspx
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using Greewf.BaseLibrary;

namespace Greewf.BaseLibrary.MVC.Validators
{

    public class PersianDateAttribute : ValidationAttribute, IClientValidatable
    {

        static PersianCalendar pcal = new PersianCalendar();

        public override bool IsValid(object value)
        {
            if (value == null)
                return true;
            return Global.IsValidDate(((string)value).Replace("/", ""));

        }

        IEnumerable<ModelClientValidationRule> IClientValidatable.GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            yield return new PersianDateValidationRule(ErrorMessage);
        }
    }

}