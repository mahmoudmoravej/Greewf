using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Greewf.BaseLibrary.ReportLoaderExtensions
{

    /// <summary>
    /// این کلاس مستقل از ریپورتینگ سرویس است
    /// بنابراین اگر می خواهید مواردی مرتبط با آن اضافه کنید از کلاس مربوطه استفاده کنید
    /// </summary>
    public class ReportSettingsBase
    {
        public ReportSettingsBase()
        {
            HumanReadablePdf = true;
            EmbedFontsInPdf = true;
        }

        public bool IsInches { get; set; }
        public double? TopMargin { get; set; }
        public double? BottomMargin { get; set; }
        public double? LeftMargin { get; set; }
        public double? RightMargin { get; set; }

        public int? StartPage { get; set; }

        public int? EndPage { get; set; }

        public bool HumanReadablePdf { get; set; }//based on PDF : http://msdn.microsoft.com/en-us/library/ms154682(v=sql.120).aspx

        public bool EmbedFontsInPdf { get; set; }//based on PDF : http://msdn.microsoft.com/en-us/library/ms154682(v=sql.120).aspx (bottom of page) and http://blogs.msdn.com/b/mariae/archive/2010/04/12/how-to-disable-this-font-embedding-in-reporting-services-2005-service-pack-3.aspx

        /// <summary>
        /// It is too important in PDF rendering when having chart.
        /// High values can result in larger files.
        /// It seems setting value to 96 or higher has no any effect on lower file size!
        /// </summary>
        public int DpiX { get; set; }

        /// <summary>
        /// It is too important in PDF rendering when having chart.
        /// High values can result in larger files.
        /// It seems setting value to 96 or higher has no any effect on lower file size!
        /// </summary>
        public int DpiY { get; set; }
    }
}
