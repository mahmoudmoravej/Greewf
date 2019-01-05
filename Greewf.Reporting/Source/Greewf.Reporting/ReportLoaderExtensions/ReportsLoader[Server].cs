using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;


namespace Greewf.Reporting
{
    public static partial class ReportsLoader
    {

  
        private static string PrepareAndGetDeviceInfo(this ServerReport report, ReportSettings settings)
        {

            var defaults = report.GetDefaultPageSettings();

            var marginTop = settings.TopMargin.HasValue ? (settings.TopMargin * (1 / 2.54)) : defaults.Margins.Top / 100.0;
            var marginBottom = settings.BottomMargin.HasValue ? (settings.BottomMargin * (1 / 2.54)) : defaults.Margins.Bottom / 100.0;
            var marginLeft = settings.LeftMargin.HasValue ? (settings.LeftMargin * (1 / 2.54)) : defaults.Margins.Left / 100.0;
            var marginRight = settings.RightMargin.HasValue ? (settings.RightMargin * (1 / 2.54)) : defaults.Margins.Right / 100.0;

            settings.EndPage = settings.EndPage ?? settings.StartPage;

            //The DeviceInfo settings should be changed based on the reportType
            // http://msdn2.microsoft.com/en-us/library/ms155397.aspx
            // http://msdn.microsoft.com/en-us/library/hh231593.aspx
            string deviceInfo =
             "<DeviceInfo>" +
             ((settings.DpiX > 0) ? "  <DpiX>" + settings.DpiX + "</DpiX>" : "") +
             ((settings.DpiY > 0) ? "  <DpiY>" + settings.DpiY + "</DpiY>" : "") +
             "  <OutputFormat>" + settings.OutputType + "</OutputFormat>" +
             "  <PageWidth>" + (defaults.IsLandscape ? defaults.PaperSize.Height : defaults.PaperSize.Width) / 100.0 + "in</PageWidth>" +
             "  <PageHeight>" + (defaults.IsLandscape ? defaults.PaperSize.Width : defaults.PaperSize.Height) / 100.0 + "in</PageHeight>" +
             "  <MarginTop>" + marginTop + "in</MarginTop>" +
             "  <MarginLeft>" + marginLeft + "in</MarginLeft>" +
             "  <MarginRight>" + marginRight + "in</MarginRight>" +
             "  <MarginBottom>" + marginBottom + "in</MarginBottom>" +
             "  <StartPage>" + (settings.StartPage ?? 0) + "</StartPage>" +
             "  <EndPage>" + (settings.EndPage ?? 0) + "</EndPage>" +
             "  <PageBreaksMode>OnEachPage</PageBreaksMode>" +
             "  <HumanReadablePDF>" + settings.HumanReadablePdf.ToString() + "</HumanReadablePDF>";

            if (!settings.EmbedFontsInPdf)
                deviceInfo += "  <EmbedFonts>None</EmbedFonts>";

            deviceInfo += "</DeviceInfo>";

            return deviceInfo;
        }


        /// <summary>
        /// Renders a local report to a Microsoft Word document on disk.
        /// </summary>
        /// <param name="report">The report</param>
        /// <param name="fileFormat">
        /// The export format (report.ListRenderingExtensions()), should be:
        /// "WORD" for DOC
        /// "WORDOPENXML" for DOCX
        /// "EXCEL" for XLS
        /// "EXCELOPENXML" for XLSX
        /// </param>
        public static byte[] ExportToFile(this ServerReport report, ReportingServiceOutputFileFormat fileFormat)
        {
            return ExportToFile(report, new ReportSettings() { OutputType = fileFormat });
        }

        public static byte[] ExportToFile(this ServerReport report, ReportSettings settings)
        {
            var deviceInfo = PrepareAndGetDeviceInfo(report, settings);

            report.SetParameters(new ReportParameter(PersianRenderer.GreewfIgnorePersianCorrectionParameterName, settings.IgnorePersianCorrection.ToString()));            

            return report.Render(GetOutputFileFormat(settings.OutputType), deviceInfo);

        }  
    }
}