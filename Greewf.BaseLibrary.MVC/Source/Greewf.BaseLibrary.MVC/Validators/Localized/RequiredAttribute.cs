using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Greewf.BaseLibrary.MVC.Validators
{
    //NOTE : don't forget to call SelfRegister method on ValidatorAttributesLocalizer.Register function
    public class LocalizedRequiredAttribute : System.ComponentModel.DataAnnotations.RequiredAttribute
    {
        public LocalizedRequiredAttribute()
        {
            this.ErrorMessageResourceType = ValidatorAttributesLocalizer.ResourceClassType;
            this.ErrorMessageResourceName = "Required";
        }

    }

    public class RequiredAttributeAdapter : DataAnnotationsModelValidator<LocalizedRequiredAttribute>
    {
        public RequiredAttributeAdapter(ModelMetadata metadata, ControllerContext context, LocalizedRequiredAttribute attribute)
            : base(metadata, context, attribute)
        {
        }

        public static void SelfRegister()
        {
            DataAnnotationsModelValidatorProvider
                .RegisterAdapter(
                    typeof(LocalizedRequiredAttribute),
                    typeof(RequiredAttributeAdapter));
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            return new[] { new ModelClientValidationRequiredRule(ErrorMessage) };
        }
    }


}
