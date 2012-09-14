using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Greewf.BaseLibrary.MVC.Validators
{
    //NOTE : don't forget to call SelfRegister method on ValidatorAttributesLocalizer.Register function
    public class LocalizedFkRequiredAttribute : System.ComponentModel.DataAnnotations.RangeAttribute
    {

        public LocalizedFkRequiredAttribute()
            : base(1, int.MaxValue)
        {
            this.ErrorMessageResourceType = ValidatorAttributesLocalizer.ResourceClassType;
            this.ErrorMessageResourceName = "FkRequiredAttribute";
        }
    }

    public class FkAttributeAdapter : DataAnnotationsModelValidator<LocalizedFkRequiredAttribute>
    {
        public FkAttributeAdapter(ModelMetadata metadata, ControllerContext context, LocalizedFkRequiredAttribute attribute)
            : base(metadata, context, attribute)
        {
        }

        public static void SelfRegister()
        {
            DataAnnotationsModelValidatorProvider
                .RegisterAdapter(
                    typeof(LocalizedFkRequiredAttribute),
                    typeof(FkAttributeAdapter));
        }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            return new[] { new ModelClientValidationRangeRule(ErrorMessage, this.Attribute.Minimum, this.Attribute.Maximum) };
        }
    }


}
