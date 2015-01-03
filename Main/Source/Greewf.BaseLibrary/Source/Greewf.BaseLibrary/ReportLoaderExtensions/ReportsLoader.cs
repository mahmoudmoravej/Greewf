using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Reporting.WebForms;
using Greewf.BaseLibrary;


namespace Greewf.BaseLibrary.ReportLoaderExtensions
{

    public class ReportSettings
    {
        public ReportSettings()
        {
            HumanReadablePdf = true;
        }

        public ReportingServiceOutputFileFormat OutputType { get; set; }
        public bool IsInches { get; set; }
        public double? TopMargin { get; set; }
        public double? BottomMargin { get; set; }
        public double? LeftMargin { get; set; }
        public double? RightMargin { get; set; }

        public bool HumanReadablePdf { get; set; }//base on PDF : http://msdn.microsoft.com/en-us/library/ms154682(v=sql.120).aspx
    }

    public static class ReportsLoader
    {
        private static Dictionary<ReportingServiceOutputFileFormat, string> _dicOutputTypes = new Dictionary<ReportingServiceOutputFileFormat, string>();

        static ReportsLoader()
        {


            _dicOutputTypes.Add(ReportingServiceOutputFileFormat.PDF, "PDF - آکروبات");
            _dicOutputTypes.Add(ReportingServiceOutputFileFormat.XLSX, "XLSX - اکسل 2007 و بعد از آن");
            _dicOutputTypes.Add(ReportingServiceOutputFileFormat.DOCX, "DOCX - وورد 2007 و بعد از آن");
            _dicOutputTypes.Add(ReportingServiceOutputFileFormat.XLS, "XLS - اکسل 2003");
            _dicOutputTypes.Add(ReportingServiceOutputFileFormat.DOC, "DOC - وورد 2003");
        }


        public static void LoadReportToAnother(LocalReport src, LocalReport dest)
        {
            src.ReportPath = dest.ReportPath;

            dest.DataSources.Clear();

            foreach (var ds in src.DataSources)
                dest.DataSources.Add(ds);
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
        public static byte[] ExportToFile(LocalReport report, ReportingServiceOutputFileFormat fileFormat)
        {
            return ExportToFile(report, new ReportSettings() { OutputType = fileFormat });
        }

        public static byte[] ExportToFile(LocalReport report, ReportSettings settings)
        {

            var defaults = report.GetDefaultPageSettings();

            var marginTop = settings.TopMargin.HasValue ? (settings.TopMargin * (1 / 2.54)) : defaults.Margins.Top / 100.0;
            var marginBottom = settings.BottomMargin.HasValue ? (settings.BottomMargin * (1 / 2.54)) : defaults.Margins.Bottom / 100.0;
            var marginLeft = settings.LeftMargin.HasValue ? (settings.LeftMargin * (1 / 2.54)) : defaults.Margins.Left / 100.0;
            var marginRight = settings.RightMargin.HasValue ? (settings.RightMargin * (1 / 2.54)) : defaults.Margins.Right / 100.0;

            //The DeviceInfo settings should be changed based on the reportType
            //http://msdn2.microsoft.com/en-us/library/ms155397.aspx
            string deviceInfo =
             "<DeviceInfo>" +
             "  <OutputFormat>" + settings.OutputType + "</OutputFormat>" +
             "  <PageWidth>" + (defaults.IsLandscape ? defaults.PaperSize.Height : defaults.PaperSize.Width) / 100.0 + "in</PageWidth>" +
             "  <PageHeight>" + (defaults.IsLandscape ? defaults.PaperSize.Width : defaults.PaperSize.Height) / 100.0 + "in</PageHeight>" +
             "  <MarginTop>" + marginTop + "in</MarginTop>" +
             "  <MarginLeft>" + marginLeft + "in</MarginLeft>" +
             "  <MarginRight>" + marginRight + "in</MarginRight>" +
             "  <MarginBottom>" + marginBottom + "in</MarginBottom>" +
             "  <PageBreaksMode>OnEachPage</PageBreaksMode>" +
             "  <HumanReadablePDF>" + settings.HumanReadablePdf.ToString() + "</HumanReadablePDF>" +
             "</DeviceInfo>";

            return report.Render(GetOutputFileFormat(settings.OutputType), deviceInfo);

        }



        private static string GetOutputFileFormat(ReportingServiceOutputFileFormat fileFormat)
        {
            switch (fileFormat)
            {
                case ReportingServiceOutputFileFormat.DOC:
                    return "WORD";
                case ReportingServiceOutputFileFormat.DOCX:
                    return "WORDOPENXML";
                case ReportingServiceOutputFileFormat.PDF:
                    return "PDF";
                case ReportingServiceOutputFileFormat.XLS:
                    return "EXCEL";
                case ReportingServiceOutputFileFormat.XLSX:
                    return "EXCELOPENXML";
                default:
                    return "";
            }
        }

        public static string GetMime(ReportingServiceOutputFileFormat fileFormat)
        {
            switch (fileFormat)
            {
                case ReportingServiceOutputFileFormat.DOC:
                    return "application/doc";
                case ReportingServiceOutputFileFormat.DOCX:
                    return "application/msword";
                case ReportingServiceOutputFileFormat.PDF:
                    return "application/pdf";
                case ReportingServiceOutputFileFormat.XLS:
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; //TODO
                case ReportingServiceOutputFileFormat.XLSX:
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                default:
                    return null;
            }
        }

        public static Dictionary<ReportingServiceOutputFileFormat, string> GetOutputFormats()
        {


            return _dicOutputTypes;
        }

        public static string GetExtention(ReportingServiceOutputFileFormat fileFormat)
        {
            switch (fileFormat)
            {
                case ReportingServiceOutputFileFormat.DOC:
                    return "doc";
                case ReportingServiceOutputFileFormat.DOCX:
                    return "docx";
                case ReportingServiceOutputFileFormat.PDF:
                    return "pdf";
                case ReportingServiceOutputFileFormat.XLS:
                    return "xls";
                case ReportingServiceOutputFileFormat.XLSX:
                    return "xlsx";
                default:
                    return "";
            }
        }

        public static string GetOutputFormatTitle(ReportingServiceOutputFileFormat format)
        {
            return _dicOutputTypes[format];
        }


    }
}