using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Mvc.Html;
using Telerik.Web.Mvc.UI;
using AutoMapper;

namespace Greewf.BaseLibrary.MVC.CustomHelpers
{
    public static partial class CustomHelper
    {

        #region ValidationFor

        public static MvcHtmlString ValidationMessageWithStarFor<TModel, TProperty>(this HtmlHelper<TModel> helper, System.Linq.Expressions.Expression<Func<TModel, TProperty>> expression)
        {
            string star = "";
            string starHtml = "<span class='field-validation-star'></span>";

            var metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);

            //if (DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes == true && !metadata.IsNullableValueType)
            //    star = starHtml;
            if (metadata.IsRequired)
                star = starHtml;

            return new MvcHtmlString(star + helper.ValidationMessageFor(expression).ToHtmlString());

        }

        #endregion

    }

}