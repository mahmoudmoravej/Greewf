using System.Reflection;

namespace Greewf.BaseLibrary.ExcelOutput
{
    public class ExcelColumnDataProvider
    {
        public PropertyInfo PropertyInfo { get; set; }
        
        public ColumnDataRetreiverDelegate ColumnDataRetreiver { get; set; }
    }
}
