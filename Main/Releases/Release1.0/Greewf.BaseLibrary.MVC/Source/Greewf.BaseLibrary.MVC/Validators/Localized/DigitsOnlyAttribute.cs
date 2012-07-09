using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Greewf.BaseLibrary.MVC.Validators
{
    //NOTE : don't forget to call SelfRegister method on ValidatorAttributesLocalizer.Register function
    public class LocalizedDigitsOnlyAttribute : System.ComponentModel.DataAnnotations.RegularExpressionAttribute
    {
        public LocalizedDigitsOnlyAttribute()
            : base(Global.DIGITSREGX)
        {
            this.ErrorMessageResourceType = ValidatorAttributesLocalizer.ResourceClassType;
            this.ErrorMessageResourceName = "DigitsOnly";
        }

    }

    public class DigitsOnlyAttributeAdapter : DataAnnotationsModelValidator<LocalizedDigitsOnlyAttribute>
    {
        public DigitsOnlyAttributeAdapter(ModelMetadata metadata, ControllerContext context, LocalizedDigitsOnlyAttribute attribute)
            : base(metadata, context, attribute)
        {
        }

        public static void SelfRegister()
        {
            DataAnnotationsModelValidatorProvider
                .RegisterAdapter(
                    typeof(LocalizedDigitsOnlyAttribute),
                    typeof(DigitsOnlyAttributeAdapter));
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            return new[] { new ModelClientValidationRegexRule(ErrorMessage, Global.DIGITSREGX) };
        }
    }


}
