using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Greewf.BaseLibrary.MVC.Validators
{
    //NOTE : don't forget to call SelfRegister method on ValidatorAttributesLocalizer.Register function
    public class LocalizedPhoneAttribute : System.ComponentModel.DataAnnotations.RegularExpressionAttribute
    {
        public LocalizedPhoneAttribute()
            : base(Global.PHONEREGX)
        {
            this.ErrorMessageResourceType = ValidatorAttributesLocalizer.ResourceClassType;
            this.ErrorMessageResourceName = "InvalidPhone";
        }

    }

    public class PhoneAttributeAdapter : DataAnnotationsModelValidator<LocalizedPhoneAttribute>
    {
        public PhoneAttributeAdapter(ModelMetadata metadata, ControllerContext context, LocalizedPhoneAttribute attribute)
            : base(metadata, context, attribute)
        {
        }

        public static void SelfRegister()
        {
            DataAnnotationsModelValidatorProvider
                .RegisterAdapter(
                    typeof(LocalizedPhoneAttribute),
                    typeof(PhoneAttributeAdapter));
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            return new[] { new ModelClientValidationRegexRule(ErrorMessage, Global.PHONEREGX) };
        }
    }


}
