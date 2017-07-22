using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Reporting.WebForms;


namespace Greewf.BaseLibrary.MVC
{
    public abstract class ReportsLoaderBase
    {

        public  void LoadReportToAnother(LocalReport src, LocalReport dest)
        {
            src.ReportPath = dest.ReportPath;

            dest.DataSources.Clear();

            foreach (var ds in src.DataSources)
                dest.DataSources.Add(ds);
        }

        public  byte[] ConvertToPdf(LocalReport report)
        {
            
            var defaults = report.GetDefaultPageSettings();
            //The DeviceInfo settings should be changed based on the reportType
            //http://msdn2.microsoft.com/en-us/library/ms155397.aspx
            string deviceInfo =
             "<DeviceInfo>" +
             "  <OutputFormat>PDF</OutputFormat>" +
             "  <PageWidth>" + defaults.PaperSize.Width / 100.0 + "in</PageWidth>" +
             "  <PageHeight>" + defaults.PaperSize.Height / 100.0 + "in</PageHeight>" +
             "  <MarginTop>" + defaults.Margins.Top / 100.0 + "in</MarginTop>" +
             "  <MarginLeft>" + defaults.Margins.Left / 100.0 + "in</MarginLeft>" +
             "  <MarginRight>" + defaults.Margins.Right / 100.0 + "in</MarginRight>" +
             "  <MarginBottom>" + defaults.Margins.Bottom / 100.0 + "in</MarginBottom>" +
             "</DeviceInfo>";


            byte[] renderedBytes;

            //Render the report
            renderedBytes = report.Render( "PDF", deviceInfo);

            return renderedBytes;

        }
    }
}