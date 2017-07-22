using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Greewf.BaseLibrary.MVC.Validators
{
    public class ValidatorAttributesLocalizer
    {
        public static Type ResourceClassType { get; internal set; }

        public static void Register(Type resourceClassType)
        {
            ResourceClassType = resourceClassType;
            RequiredAttributeAdapter.SelfRegister();
            PhoneAttributeAdapter.SelfRegister();
            StringLengthAttributeAdapter.SelfRegister();
            DigitsOnlyAttributeAdapter.SelfRegister();
            RangeAttributeAdapter.SelfRegister();
            FkAttributeAdapter.SelfRegister();
        }
    }
}
