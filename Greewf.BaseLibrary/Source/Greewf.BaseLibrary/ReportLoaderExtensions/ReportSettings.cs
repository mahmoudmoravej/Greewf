using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greewf.BaseLibrary.ReportLoaderExtensions
{

    /// <summary>
    /// این کلاس وابسته به ریپورتینگ سرویس است
    /// </summary>
    public class ReportSettings : ReportSettingsBase
    {
        public ReportingServiceOutputFileFormat OutputType { get; set; }  
    }
}
