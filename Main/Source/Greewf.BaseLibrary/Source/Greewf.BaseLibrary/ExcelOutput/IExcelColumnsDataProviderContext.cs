using System;
using System.Collections.Generic;

namespace Greewf.BaseLibrary.ExcelOutput
{
    public delegate void ColumnDataRetreiverDelegate(ExcelColumnLayout columnInfo, object row, ref object fieldValue);

    public interface IExcelColumnsDataProviderContext
    {
        /// <summary>
        /// the second parameter of function is the value of a cell with specified passed "Type".
        /// </summary>
        /// <returns></returns>
        Dictionary<Type, ColumnDataRetreiverDelegate> GetColumnsDataProviders();
    }
}