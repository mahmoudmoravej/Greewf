﻿using Greewf.BaseLibrary.MVC.Security;
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


    public class ColumnDataProvider
    {
        public PropertyInfo PropertyInfo { get; set; }
        public Func<ColumnLayout, object, string> ArrayColumnDataRetreiver { get; set; }
    }

    public interface IArrayColumnsDataProviderContext
    {
        Dictionary<Type, Func<ColumnLayout, object, string>> GetArrayColumnsDataProviders();
    }

    public class ExcelOutputAttribute : ActionFilterAttribute
    {

        private long? _permissionObject = null;
        private long? _permissions = null;
        private Type _arrayColumnsDataProviderContext;

        public ExcelOutputAttribute()
        {
        }

        public ExcelOutputAttribute(long permissionObject, long permissions)
        {
            _permissionObject = permissionObject;
            _permissions = permissions;
        }


        public ExcelOutputAttribute(long permissionObject, long permissions, Type arrayColumnsDataProviderContext)
        {
            _permissionObject = permissionObject;
            _permissions = permissions;
            _arrayColumnsDataProviderContext = arrayColumnsDataProviderContext;
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
            var propertiesDataProviders = new Dictionary<string, ColumnDataProvider>();

            int idx = 0;
            int columnCount = 0;
            var headerFont = workbook.CreateFont();
            headerFont.Boldweight = (short)FontBoldWeight.BOLD;
            headerFont.FontHeightInPoints = 14;

            var rowsFont = workbook.CreateFont();
            rowsFont.Boldweight = (short)FontBoldWeight.NORMAL;
            rowsFont.FontHeightInPoints = 12;

            Dictionary<Type, Func<ColumnLayout, object, string>> arrayColumnsDataProviders = null;

            if (_arrayColumnsDataProviderContext != null)
            {
                object context;
                try
                {
                    context = Activator.CreateInstance(_arrayColumnsDataProviderContext);
                }
                catch (Exception exp)
                {
                    throw new Exception("Greewf : Error in creating '" + _arrayColumnsDataProviderContext.Name + "' Type. Details : " + exp.Message);
                }
                if (context is IArrayColumnsDataProviderContext == false)
                    throw new Exception("Greewf : The passed type for 'arrayColumnsDataProviderContext' parameter of 'ExcelOutput' attribute should inherit from 'IArrayColumnsDataProviderContext'");

                arrayColumnsDataProviders = (context as IArrayColumnsDataProviderContext).GetArrayColumnsDataProviders();
            }


            foreach (var item in columnLayouts)
            {
                if (string.IsNullOrWhiteSpace(item.Id)) continue;
                columnCount++;
                var cell = headerRow.CreateCell(idx++);
                cell.SetCellValue(item.Title);
                cell.CellStyle.SetFont(headerFont);

                if (propertiesDataProviders.ContainsKey(item.Id + item.Title))
                    throw new Exception("Greewf : The combination of column header('" + item.Id + "' and '" + item.Title + "') is not unique. ExcelOutput needs this combination to be unique ");
                else
                {
                    var colDataProvider = new ColumnDataProvider();
                    colDataProvider.PropertyInfo = rowType.GetProperty(item.Id);

                    if (colDataProvider.PropertyInfo.PropertyType.IsArray ||
                        (colDataProvider.PropertyInfo.PropertyType.IsGenericType && colDataProvider.PropertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                    {
                        if (arrayColumnsDataProviders == null)
                            throw new Exception("Greewf : You should pass 'ArrayColumnDataProviders' parameter for 'ExcelOutput' attribute when you have some array columns.");

                        var arrayDataRetreiver = arrayColumnsDataProviders.FirstOrDefault(f => f.Key == colDataProvider.PropertyInfo.PropertyType);

                        if (arrayDataRetreiver.Key == null)
                            throw new Exception("Greewf : 'ArrayColumnDataProviders' paramater (of 'ExcelOutput' attribute)  doesn't have any relative function for array of  type : " + colDataProvider.PropertyInfo.PropertyType.ToString());
                        else
                            colDataProvider.ArrayColumnDataRetreiver = arrayDataRetreiver.Value;
                    }

                    propertiesDataProviders.Add(item.Id + item.Title, colDataProvider);
                }
            }


            //(Optional) freeze the header row so it is not scrolled
            sheet.CreateFreezePane(0, 1, 0, 1);
            var lstIgnoreColumnAutoSize = new List<int>();

            int rowNumber = 1;

            foreach (var rowData in data)
            {
                var row = sheet.CreateRow(rowNumber++);

                idx = 0;
                foreach (var item in columnLayouts)
                {
                    if (string.IsNullOrWhiteSpace(item.Id)) continue;
                    var colDataProvider = propertiesDataProviders[item.Id + item.Title];
                    var propertyValue = colDataProvider.PropertyInfo.GetValue(rowData, null);
                    string value = null;

                    if (colDataProvider.ArrayColumnDataRetreiver != null)
                        value = colDataProvider.ArrayColumnDataRetreiver(item, propertyValue);
                    else if (colDataProvider.PropertyInfo.PropertyType.IsAssignableFrom(typeof(DateTime)))
                        value = Global.DisplayDateTime((DateTime?)propertyValue);
                    else if (propertyValue is bool || propertyValue is bool?)
                        value = (bool?)propertyValue == true ? "بلی" : (bool?)propertyValue == false ? "خیر" : "";
                    else
                        value = propertyValue == null ? "" : propertyValue.ToString();

                    //NOTE : I don't know what the problem is but all the cell style are indeed ONE instance object!!!
                    var cell = row.CreateCell(idx++);
                    cell.CellStyle.SetFont(rowsFont);
                    cell.SetCellValue(value);

                    if ((value ?? "").Length > 200)
                    {
                        sheet.SetColumnWidth(idx - 1, 150 * 256);
                        cell.CellStyle.WrapText = true;
                        lstIgnoreColumnAutoSize.Add(idx - 1);
                        ff sheet.SetDefaultColumnStyle(, workbook.CreateCellStyle());
                    }
                }
                
            }


            for (int i = 0; i < columnCount; i++)
            {
                if (!lstIgnoreColumnAutoSize.Contains(i))
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
            //TODO !!! : not supports ARRAY COLUMNS!!!
            //TODO !!! : not supports ARRAY COLUMNS!!!
            //TODO !!! : not supports ARRAY COLUMNS!!!

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
