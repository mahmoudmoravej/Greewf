using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Greewf.BaseLibrary.MVC.Validators
{
    //NOTE : don't forget to call SelfRegister method on ValidatorAttributesLocalizer.Register function
    public class LocalizedStringLengthAttribute : System.ComponentModel.DataAnnotations.StringLengthAttribute
    {
        public LocalizedStringLengthAttribute(int maximumLength)
            : this(0, maximumLength)
        {
        }

        public LocalizedStringLengthAttribute(int minimumLength, int maximumLength)
            : base(maximumLength)
        {
            this.MinimumLength = minimumLength;

            this.ErrorMessageResourceType = ValidatorAttributesLocalizer.ResourceClassType;

            if (minimumLength == MaximumLength)
                this.ErrorMessageResourceName = "ExactLengthError";
            else if (minimumLength <= 0)
                this.ErrorMessageResourceName = "MaxLengthError";
            else
                this.ErrorMessageResourceName = "LengthError";
        }

    }

    public class StringLengthAttributeAdapter : DataAnnotationsModelValidator<LocalizedStringLengthAttribute>
    {
        public StringLengthAttributeAdapter(ModelMetadata metadata, ControllerContext context, LocalizedStringLengthAttribute attribute)
            : base(metadata, context, attribute)
        {
        }

        public static void SelfRegister()
        {
            DataAnnotationsModelValidatorProvider
                .RegisterAdapter(
                    typeof(LocalizedStringLengthAttribute),
                    typeof(StringLengthAttributeAdapter));
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            return new[] { new ModelClientValidationStringLengthRule(ErrorMessage, this.Attribute.MinimumLength, this.Attribute.MaximumLength) };
        }
    }


}
