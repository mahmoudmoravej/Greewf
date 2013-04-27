using NPOI.HSSF.UserModel;
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

namespace Greewf.BaseLibrary.MVC.TelerikExtentions
{

    public class ColumnLayout
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
    }

    public class ExcelOutputAttribute : ActionFilterAttribute
    {



        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (HttpContext.Current.Request["exportToExcel"] != null)
            {
                GridModel oldModel = (GridModel)filterContext.Controller.ViewData.Model;
                var filter = HttpContext.Current.Request["filter"];
                var orderBy = HttpContext.Current.Request["orderBy"];
                var model = Telerik.Web.Mvc.Extensions.QueryableExtensions.ToGridModel(oldModel.Data.AsQueryable(), 1, 0, orderBy, string.Empty, filter);
                var ser = new JavaScriptSerializer();
                var layouts = ser.Deserialize(HttpContext.Current.Request["layout"], typeof(List<ColumnLayout>)) as List<ColumnLayout>;

                filterContext.Result = ExportToExcel(model.Data.AsQueryable(), layouts);

                //base.OnResultExecuted(filterContext);

            }
            else
                base.OnActionExecuted(filterContext);

        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);
        }


        public FileResult ExportToExcel(IQueryable data, List<ColumnLayout> columnLayouts)
        {
            var rowType = data.GetType().GetGenericArguments()[0];
            var o = Activator.CreateInstance(rowType);
            //var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => o, rowType);

            //Create new Excel workbook
            var workbook = new HSSFWorkbook();

            //Create new Excel sheet
            var sheet = workbook.CreateSheet();

            //Create a header row
            var headerRow = sheet.CreateRow(0);
            var propertiesDataProviders = new Dictionary<string, PropertyInfo>();

            int idx = 0;
            int columnCount = 0;
            var headerFont = workbook.CreateFont();
            headerFont.Boldweight = (short)FontBoldWeight.BOLD;
            headerFont.FontHeightInPoints = 14;

            var rowsFont = workbook.CreateFont();
            rowsFont.Boldweight = (short)FontBoldWeight.NORMAL;
            rowsFont.FontHeightInPoints = 12;


            foreach (var item in columnLayouts)
            {
                if (string.IsNullOrWhiteSpace(item.Id)) continue;
                columnCount++;
                var cell = headerRow.CreateCell(idx++);
                cell.SetCellValue(item.Title);
                cell.CellStyle.SetFont(headerFont);
                propertiesDataProviders.Add(item.Id, rowType.GetProperty(item.Id));
            }


            //(Optional) freeze the header row so it is not scrolled
            sheet.CreateFreezePane(0, 1, 0, 1);

            int rowNumber = 1;

            foreach (var rowData in data)
            {
                var row = sheet.CreateRow(rowNumber++);

                idx = 0;
                foreach (var item in columnLayouts)
                {
                    if (string.IsNullOrWhiteSpace(item.Id)) continue;
                    var propertyInfo = propertiesDataProviders[item.Id];
                    var propertyValue = propertyInfo.GetValue(rowData, null);
                    string value = null;

                    if (propertyInfo.PropertyType.IsAssignableFrom(typeof(DateTime)))
                        value = Global.DisplayDateTime((DateTime?)propertyValue);
                    else
                        value = propertyValue == null ? "" : propertyValue.ToString();

                    var cell = row.CreateCell(idx++);
                    cell.CellStyle.SetFont(rowsFont);
                    cell.SetCellValue(value);
                }
            }


            for (int i = 0; i < columnCount; i++)
            {
                sheet.AutoSizeColumn(i);
            }
            sheet.PrintSetup.LeftToRight = false;

            //Write the workbook to a memory stream
            MemoryStream output = new MemoryStream();
            workbook.Write(output);

            //Return the result to the end user
            var x = new FileContentResult(output.ToArray(), "application/vnd.ms-excel");
            x.FileDownloadName = string.Format("GridReport{0}.xls", Greewf.BaseLibrary.Global.DisplayDateTime(DateTime.Now).Replace(":", "-").Replace("/", "-"));

            return x;
        }

        public FileResult ExportToExcel(IQueryable data)
        {
            var rowType = data.GetType().GetGenericArguments()[0];
            var o = Activator.CreateInstance(rowType);
            var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => o, rowType);

            //Create new Excel workbook
            var workbook = new HSSFWorkbook();

            //Create new Excel sheet
            var sheet = workbook.CreateSheet();

            //Create a header row
            var headerRow = sheet.CreateRow(0);
            var propertiesDataProviders = new Dictionary<string, PropertyInfo>();

            int idx = 0;
            int columnCount = 0;
            var font = workbook.CreateFont();
            font.Boldweight = (short)FontBoldWeight.BOLD;

            foreach (var item in metadata.Properties)
            {
                if (!string.IsNullOrWhiteSpace(item.DisplayName))
                {
                    columnCount++;
                    var cell = headerRow.CreateCell(idx++);
                    cell.SetCellValue(item.DisplayName);
                    propertiesDataProviders.Add(item.PropertyName, rowType.GetProperty(item.PropertyName));
                }
            }


            //(Optional) freeze the header row so it is not scrolled
            sheet.CreateFreezePane(0, 1, 0, 1);

            int rowNumber = 1;

            foreach (var rowData in data)
            {
                var row = sheet.CreateRow(rowNumber++);

                idx = 0;
                foreach (var item in metadata.Properties)
                {
                    if (!string.IsNullOrWhiteSpace(item.DisplayName))
                    {
                        var propertyInfo = propertiesDataProviders[item.PropertyName];
                        var propertyValue = propertyInfo.GetValue(rowData, null);
                        string value = null;

                        if (typeof(DateTime) == propertyInfo.PropertyType)
                            value = Global.DisplayDateTime((DateTime?)propertyValue);
                        else
                            value = propertyValue == null ? "" : propertyValue.ToString();

                        row.CreateCell(idx++).SetCellValue(value);
                    }
                }
            }


            for (int i = 0; i < columnCount; i++)
            {
                sheet.GetRow(0).Cells[i].CellStyle.SetFont(font);
                sheet.AutoSizeColumn(i);
            }
            sheet.PrintSetup.LeftToRight = false;

            //Write the workbook to a memory stream
            MemoryStream output = new MemoryStream();
            workbook.Write(output);

            //Return the result to the end user
            var x = new FileContentResult(output.ToArray(), "application/vnd.ms-excel");
            x.FileDownloadName = string.Format("GridReport{0}.xls", Greewf.BaseLibrary.Global.DisplayDateTime(DateTime.Now).Replace(":", "-").Replace("/", "-"));

            return x;
        }

    }
}
