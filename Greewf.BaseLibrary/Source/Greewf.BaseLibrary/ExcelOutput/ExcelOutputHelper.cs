using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Greewf.BaseLibrary.ExcelOutput
{
    public static class ExcelOutputHelper
    {
        public static MemoryStream ExportToExcel(IQueryable data, List<ExcelColumnLayout> columnLayouts, Type columnsDataProviderContext, bool isTree, int ExcelOutputLevel, IValidationDictionary validationDictionary = null, bool useExcel2007AndAbove = false)
        {
            var rowType = data.GetType().GetGenericArguments()[0];
            var o = Activator.CreateInstance(rowType);
            //var metadata = ModelMetadataProviders.Current.GetMetadataForType(() => o, rowType);

            //Create new Excel workbook
            //NOTE : XSSFWorkbook is for xslx format but its buggy in this version (2.1.3). 
            IWorkbook workbook;

            if (useExcel2007AndAbove)
                workbook = new XSSFWorkbook();
            else
                workbook = new HSSFWorkbook();

            //Create new Excel sheet
            var sheet = workbook.CreateSheet();

            //Create a header row
            var headerRow = sheet.CreateRow(0);
            var propertiesDataProviders = new Dictionary<string, ExcelColumnDataProvider>();

            int idx = 0;
            int columnCount = 0;

            var rowsFont = workbook.CreateFont();
            rowsFont.Boldweight = (short)FontBoldWeight.Normal;
            rowsFont.FontHeightInPoints = 12;

            Dictionary<Type, ColumnDataRetreiverDelegate> columnsDataProviders = null;

            if (columnsDataProviderContext != null)
            {
                object context;
                try
                {
                    context = Activator.CreateInstance(columnsDataProviderContext);
                }
                catch (Exception exp)
                {
                    throw new Exception("Greewf : Error in creating '" + columnsDataProviderContext.Name + "' Type. Details : " + exp.Message);
                }
                if (context is IExcelColumnsDataProviderContext == false)
                    throw new Exception("Greewf : The passed type for 'columnsDataProviderContext' parameter of 'ExcelOutput' attribute should inherit from 'IColumnsDataProviderContext'");

                columnsDataProviders = (context as IExcelColumnsDataProviderContext).GetColumnsDataProviders();
            }

            // header style
            var headerCellStyle = workbook.CreateCellStyle();
            var headerFont = workbook.CreateFont();
            headerFont.Boldweight = (short)FontBoldWeight.Bold;
            headerFont.FontHeightInPoints = 12;
            headerCellStyle.SetFont(headerFont);
            headerCellStyle.FillBackgroundColor = HSSFColor.DarkBlue.Grey80Percent.Index;
            headerCellStyle.VerticalAlignment = VerticalAlignment.Center;
            headerRow.Height = 40 * 20;

            foreach (var item in columnLayouts)
            {
                if (string.IsNullOrWhiteSpace(item.Id)) continue;
                columnCount++;
                var cell = headerRow.CreateCell(idx++);
                cell.SetCellValue(item.Title);
                cell.CellStyle = headerCellStyle;

                if (propertiesDataProviders.ContainsKey(item.Id + item.Title))
                    throw new Exception("Greewf : The combination of column header('" + item.Id + "' and '" + item.Title + "') is not unique. ExcelOutput needs this combination to be unique ");
                else
                {
                    var colDataProvider = new ExcelColumnDataProvider();
                    colDataProvider.PropertyInfo = rowType.GetProperty(item.Id);

                    if (colDataProvider.PropertyInfo != null &&//PropertyInfo is null for unrelated columns (it presents in client only)
                        (colDataProvider.PropertyInfo.PropertyType.IsArray ||
                        (colDataProvider.PropertyInfo.PropertyType.IsGenericType && colDataProvider.PropertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))))
                    {
                        if (columnsDataProviders == null)
                            throw new Exception("Greewf : You should pass 'ColumnDataProviders' parameter for 'ExcelOutput' attribute when you have some array columns.");

                        var colDataRetreiver = columnsDataProviders.FirstOrDefault(f => f.Key == colDataProvider.PropertyInfo.PropertyType);

                        if (colDataRetreiver.Key == null)
                            throw new Exception("Greewf : 'ColumnDataProviders' paramater (of 'ExcelOutput' attribute)  doesn't have any relative function for array of  type : " + colDataProvider.PropertyInfo.PropertyType.ToString());
                        else
                            colDataProvider.ColumnDataRetreiver = colDataRetreiver.Value;

                    }
                    else if (columnsDataProviders != null && colDataProvider.PropertyInfo != null)//simple column type providers if any
                    {
                        var provider = columnsDataProviders.FirstOrDefault(f => f.Key == colDataProvider.PropertyInfo.PropertyType);
                        if (provider.Key != null)
                            colDataProvider.ColumnDataRetreiver = provider.Value;
                    }

                    propertiesDataProviders.Add(item.Id + item.Title, colDataProvider);
                }
            }


            //(Optional) freeze the header row so it is not scrolled
            sheet.CreateFreezePane(0, 1, 0, 1);
            var lstIgnoreColumnAutoSize = new List<int>();

            // extra width cell style
            var extraWidthCellStyle = workbook.CreateCellStyle();
            extraWidthCellStyle.WrapText = true;

            int rowNumber = useExcel2007AndAbove ? 0 : 1;

            ArrayList finalData = new ArrayList();
            if (isTree)
            {
                finalData = flatTreeData(data, ExcelOutputLevel, validationDictionary);
                List<object> results = finalData.Cast<object>()
                                                    .ToList();
                data = results.AsQueryable();
            }


            foreach (var rowData in data)
            {
                rowNumber++;
                if (validationDictionary != null && rowNumber == 65537 && workbook is HSSFWorkbook)//excel 2003 does not supports rows more than 65534
                {
                    validationDictionary.AddError("", "امکان ارسال بیش از 65535 ردیف به اکسل نمی باشد ");//TODO:we should change it to 
                    return null;
                }
                var row = sheet.CreateRow(rowNumber);

                idx = 0;
                foreach (var item in columnLayouts)
                {
                    if (string.IsNullOrWhiteSpace(item.Id)) continue;
                    var colDataProvider = propertiesDataProviders[item.Id + item.Title];
                    var propertyValue = colDataProvider.PropertyInfo == null ? null : colDataProvider.PropertyInfo.GetValue(rowData, null);
                    ICell cell = null;
                    string value = null;

                    //if (colDataProvider.PropertyInfo == null)//for columns that are unrelated to server query. indeed they are only present in client
                    //{
                    //    cell = row.CreateCell(idx++, CellType.String);
                    //    value = "";
                    //    cell.SetCellValue(value);
                    //}
                    if (colDataProvider.ColumnDataRetreiver != null)
                        colDataProvider.ColumnDataRetreiver(item, rowData, ref propertyValue);


                    if (propertyValue is DateTime || propertyValue is DateTime?)
                    {
                        cell = row.CreateCell(idx++, CellType.String);
                        if (propertyValue != null)
                        {
                            var dt = (DateTime)propertyValue;
                            if (dt == dt.Date)
                                value = Global.DisplayDate(dt);
                            else
                                value = Global.DisplayDateTime(dt);
                        }
                        cell.SetCellValue(value ?? "");
                    }
                    else if (propertyValue is bool || propertyValue is bool?)
                    {
                        cell = row.CreateCell(idx++, CellType.Boolean);
                        value = (bool?)propertyValue == true ? "بلی" : (bool?)propertyValue == false ? "خیر" : "";
                        cell.SetCellValue(value);
                    }
                    else if (propertyValue is int ||
                             propertyValue is int? ||
                             propertyValue is decimal ||
                             propertyValue is decimal? ||
                             propertyValue is double ||
                             propertyValue is double? ||
                             propertyValue is short ||
                             propertyValue is short? ||
                             propertyValue is float ||
                             propertyValue is float?)
                    {
                        cell = row.CreateCell(idx++, CellType.Numeric);
                        if (propertyValue != null) cell.SetCellValue(Convert.ToDouble(propertyValue));
                    }
                    else
                    {

                        cell = row.CreateCell(idx++, CellType.String);
                        value = propertyValue == null ? "" : propertyValue.ToString();
                        cell.SetCellValue(value);

                    }


                    //NOTE : I don't know what the problem is but all the cell style are indeed ONE instance object!!!

                    cell.CellStyle.SetFont(rowsFont);


                    if ((value ?? "").Length > 200)
                    {
                        cell.CellStyle = extraWidthCellStyle;

                        lstIgnoreColumnAutoSize.Add(idx - 1);
                    }
                }

            }



            //row.SetAttribute(new OpenXmlAttribute("outlineLevel", string.Empty, "1"));




            for (int i = 0; i < columnCount; i++)
            {
                if (!lstIgnoreColumnAutoSize.Contains(i))
                    sheet.AutoSizeColumn(i);
                else
                    sheet.SetColumnWidth(i, 150 * 256);

            }

            //sheet.IsRightToLeft = true;
            sheet.IsRightToLeft = true;
            sheet.PrintSetup.LeftToRight = false;
            groupData(finalData, sheet, isTree);

            //Write the workbook to a memory stream
            MemoryStream output = new MemoryStream();
            workbook.Write(output);

            output.Close();

            return output;
        }
        // to get the last item node in a subtree
        private static int getLastItemIndex(List<object> myData, object item)
        {
            var lastItem = item;
            bool isLeaf = false;
            int lastItemIndex= myData.IndexOf(item);
            while (!isLeaf)
            {
                var resultProperties = item.GetType().GetProperties();
                foreach (var property in resultProperties)
                {

                    if (property.Name == "Children")
                    {
                        var children = property.GetValue(lastItem);
                        var List = (object[])children;
                        if (List != null && List.Any())
                        {
                            lastItem = List.LastOrDefault();

                            if (myData.IndexOf(lastItem) > 0)
                            {
                                lastItemIndex = myData.IndexOf(lastItem);
                            }
                            else { isLeaf = true; }
                        }
                        else { isLeaf = true; }

                    }
                }

            };
            return lastItemIndex;
        }

        // add child node to item data list
        private static ArrayList flatTreeData(IQueryable data, int ExcelOutputLevel, IValidationDictionary validationDictionary)
        {

            ArrayList finalResults = ConvertQueryToList(data);
            ArrayList results = ConvertQueryToList(data);

            if (results.Count > 0)
            {

                Type resultsType = results[0].GetType();
                PropertyInfo[] resultProperties = resultsType.GetProperties();

                foreach (var item in results)
                {
                    int counter = 1;
                    foreach (var property in resultProperties)
                    {
                        if (property.Name == "Children")
                        {
                            var children = property.GetValue(item);
                            var List = (object[])children;
                            if (List != null && List.Any())
                            {
                                foreach (var node in List)
                                {
                                    string ChildTitle = node.GetType().GetProperty("Title").GetValue(node).ToString();
                                    string spaces = new string(' ', counter * 8);
                                    ChildTitle = spaces + ChildTitle;
                                    node.GetType().GetProperty("Title").SetValue(node, ChildTitle);
                                }
                                var itemindex = finalResults.IndexOf(item);
                                //add the first Level to result

                                if (ExcelOutputLevel > counter)
                                {

                                    finalResults.InsertRange(itemindex + 1, List);

                                    counter++;
                                    if (ExcelOutputLevel > counter)
                                    {
                                        foreach (var ch in List)
                                        {
                                            var ar1 = addChildToResult(ch, finalResults, ExcelOutputLevel, counter, validationDictionary);
                                        }
                                    }
                                }

                            }
                        };
                    }

                }
            }
            return finalResults;
        }

        private static ArrayList addChildToResult(object ch, ArrayList fResult, int ExcelOutputLevel, int counter, IValidationDictionary validationDictionary)
        {

            //int counter=1;
            ArrayList ar = new ArrayList();
            Type resultsType = ch.GetType();
            PropertyInfo[] resultProperties = resultsType.GetProperties();

            foreach (var property in resultProperties)
            {
                if (property.Name == "Children")
                {
                    var children = property.GetValue(ch);
                    var List = (object[])children;
                    if (List != null && List.Any())
                    {
                        foreach (var child in List)
                        {
                            string ChildTitle = child.GetType().GetProperty("Title").GetValue(child).ToString();
                            string spaces = new string(' ', counter*8);
                            ChildTitle = spaces + ChildTitle;
                            child.GetType().GetProperty("Title").SetValue(child, ChildTitle);
                        }

                        var itemindex = fResult.IndexOf(ch);
                        fResult.InsertRange(itemindex + 1, List);
                        counter++;
                        if (counter > 6)
                        {
                            validationDictionary.AddError("", "امکان خروجی برای داده های بیشتر از 7 سطح مقدور نمیباشد لذا داده های با سطح بالاتر در خروجی  حذف شده است ");

                        }
                        if (ExcelOutputLevel > counter)
                        {
                            foreach (var che in List)
                            {

                                addChildToResult(che, fResult, ExcelOutputLevel, counter, validationDictionary);

                            }
                        }

                    }
                };
            }
            return fResult;
        }

        private static void groupData(ArrayList finalData, ISheet sheet, bool isTree)
        {

            if (isTree)
            {
                List<object> finalDataList = finalData.OfType<object>().ToList();
                foreach (var item in finalDataList)
                {
                    bool HasChild = false;
                    var resultProperties = item.GetType().GetProperties();
                    foreach (var property in resultProperties)
                    {
                        if (property.Name == "Children")
                        {
                            var children = property.GetValue(item);
                            var ListChild = (object[])children;
                            if (ListChild != null && ListChild.Any())
                            {
                                HasChild = true;
                            }
                        }

                    }
                    //item.GetType().GetProperty("Children").GetValue(item) != null
                    if (HasChild)
                    {
                        int firstItemindex = finalDataList.IndexOf(item) + 2;
                        int lastchildIndex = getLastItemIndex(finalDataList, item);
                        sheet.GroupRow(firstItemindex, lastchildIndex + 1);
                      
                    }
                }
                sheet.RowSumsBelow = false;
            }

        }


        public static ArrayList ConvertQueryToList(IQueryable query)
        {
            ArrayList results = new ArrayList();
            results.AddRange(query.Cast<object>().ToList());
            return results;
        }


    }
}
