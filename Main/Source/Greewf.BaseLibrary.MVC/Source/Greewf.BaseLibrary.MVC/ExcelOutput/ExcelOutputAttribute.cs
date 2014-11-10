using Greewf.BaseLibrary.ExcelOutput;
using Greewf.BaseLibrary.MVC.Security;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Telerik.Web.Mvc;

namespace Greewf.BaseLibrary.MVC.ExcelOutput
{
    public class ExcelOutputAttribute : ActionFilterAttribute
    {

        private long? _permissionObject = null;
        private long? _permissions = null;
        private Type _columnsDataProviderContext;

        public ExcelOutputAttribute()
        {
        }

        public ExcelOutputAttribute(long permissionObject, long permissions)
        {
            _permissionObject = permissionObject;
            _permissions = permissions;
        }


        public ExcelOutputAttribute(long permissionObject, long permissions, Type columnsDataProviderContext)
        {
            _permissionObject = permissionObject;
            _permissions = permissions;
            _columnsDataProviderContext = columnsDataProviderContext;
        }


        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (HttpContext.Current.Request["exportToExcel"] != null)
            {
                if (_permissionObject.HasValue && CurrentUserBase.GetActiveInstance().HasPermission(_permissionObject.Value, _permissions.Value) == false)
                    throw new SecurityException(_permissionObject.Value);

                GridModel oldModel = (GridModel)filterContext.Controller.ViewData.Model;
                var filter = HttpContext.Current.Request["filter"];
                var orderBy = HttpContext.Current.Request["orderBy"];
                var model = Telerik.Web.Mvc.Extensions.QueryableExtensions.ToGridModel(oldModel.Data.AsQueryable(), 1, 0, orderBy, string.Empty, filter);
                var ser = new JavaScriptSerializer();
                var layouts = ser.Deserialize(HttpContext.Current.Request["layout"], typeof(List<ExcelColumnLayout>)) as List<ExcelColumnLayout>;

                //Return the result to the end user
                var output = ExcelOutputHelper.ExportToExcel(model.Data.AsQueryable(), layouts, _columnsDataProviderContext);
                var result = new FileContentResult(output.ToArray(), "application/vnd.ms-excel");
                result.FileDownloadName = string.Format("GridReport{0}.xls", Greewf.BaseLibrary.Global.DisplayDateTime(DateTime.Now).Replace(":", "-").Replace("/", "-"));

                filterContext.Result = result;

                //base.OnResultExecuted(filterContext);

            }
            else
                base.OnActionExecuted(filterContext);

        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);
        }

    }
}
