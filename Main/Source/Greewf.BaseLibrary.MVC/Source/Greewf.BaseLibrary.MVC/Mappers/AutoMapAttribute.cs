using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Greewf.BaseLibrary.MVC.Mappers
{
    using System;
    using System.Web.Mvc;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AutoMapAttribute : ActionFilterAttribute
    {
        public Type SourceType { get; private set; }
        public Type DestType { get; private set; }

        public AutoMapAttribute(Type sourceType, Type destType)
        {
            SourceType = sourceType;
            DestType = destType;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            var controller = filterContext.Controller as IModelMapperController;
            if (controller == null)
            {
                return;
            }
            var model = filterContext.Controller.ViewData.Model;
            if (model != null)
            {
                var modelType = model.GetType();
                if (/*modelType.IsAssignableFrom(SourceType) &&*/ SourceType.IsAssignableFrom(modelType))
                //TODO : it should be exactly the same type but I didn't change it because of unexpected side-effects may happen in old projects ( not sure!) : one of the main reason is EF-Code-First proxy classes
                {
                    var viewModel = controller.ModelMapper.Map(model, DestType);
                    filterContext.Controller.ViewData.Model = viewModel;
                }
            }

        }
    }
}