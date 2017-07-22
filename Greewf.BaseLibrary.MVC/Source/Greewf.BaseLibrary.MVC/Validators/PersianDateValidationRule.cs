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

    public class PersianDateValidationRule : ModelClientValidationRule
    {

        public PersianDateValidationRule(string errorMessage)
        {
            ErrorMessage = errorMessage;
            ValidationType = "persiandate";
        }

    }

}