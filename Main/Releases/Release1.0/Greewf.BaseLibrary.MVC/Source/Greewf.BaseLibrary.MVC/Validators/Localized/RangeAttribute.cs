using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Greewf.BaseLibrary.MVC.Validators
{
    //NOTE : don't forget to call SelfRegister method on ValidatorAttributesLocalizer.Register function
    public class LocalizedRangeAttribute : System.ComponentModel.DataAnnotations.RangeAttribute
    {

        public LocalizedRangeAttribute(int minimum, int maximum)
            : base(minimum, maximum)
        {

        }

        public LocalizedRangeAttribute(double minimum, double maximum)
            : base(minimum, maximum)
        {

        }

        public LocalizedRangeAttribute(Type type, string minimum, string maximum)
            : base(type, minimum, maximum)
        {

        }

        public LocalizedRangeAttribute(int minimum)
            : base(minimum, int.MaxValue)
        {
            this.ErrorMessageResourceType = ValidatorAttributesLocalizer.ResourceClassType;
            this.ErrorMessageResourceName = "InvalidMinRange";
        }

    }

    public class RangeAttributeAdapter : DataAnnotationsModelValidator<LocalizedRangeAttribute>
    {
        public RangeAttributeAdapter(ModelMetadata metadata, ControllerContext context, LocalizedRangeAttribute attribute)
            : base(metadata, context, attribute)
        {
        }

        public static void SelfRegister()
        {
            DataAnnotationsModelValidatorProvider
                .RegisterAdapter(
                    typeof(LocalizedRangeAttribute),
                    typeof(RangeAttributeAdapter));
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            return new[] { new ModelClientValidationRangeRule(ErrorMessage, this.Attribute.Minimum, this.Attribute.Maximum) };
        }
    }


}
