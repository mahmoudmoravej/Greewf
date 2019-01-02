using Microsoft.Reporting.WebForms;
using System;
using System.IO;
using System.Linq;

using System.Xml.Linq;

namespace Greewf.Reporting
{
    public static class LocalReportExtensions
    {


        /// <summary>
        /// از ثابت های زیر می توانید در طراحی گزارش خود استفاده کنید و فرآیند صحیح سازی را با توجه به نیاز شخصی سازی نمایید
        ///  Constants : 
        ///  GreewfIgnoreGlobalVariablesAtStart = متغیرهای عمومی مانند شماره صفحه خودکار را مستثنی می کند. فقط مناسب جایی است که می خواهید گزارش "آفیس وورد" بدهید و شماره صفحه خودکار داردید. چراکه اگر متغیر گلوبال مربوطه توسط تابعی تغییر کند دیگر آن متغیر کار نمی کند و همیشه شماره 1 را به عنوان شماره صفحه درج می کند
        ///  GreewfConvertSlashBetweenDigitsToDecimalSeprator = اسلش میان اعداد را به ممیز اعشار تبدیل کند . این موضوع فقط برای فونت های اچ.ام پشتیبانی می شود
        ///  GreewfIgnoreCorrection = عملیات صحیح سازی را بر روی آن تگ بطور خاص انجام نمی دهد
        /// </summary>
        public static void LoadReport(this LocalReport report, string path, ReportCorrectionMode reportCorrectionMode)
        {

            if (reportCorrectionMode == ReportCorrectionMode.None)
            {
                report.ReportPath = path;
                return;
            }

            var file = new FileInfo(path);

            var stream = LoadReportDefinition(file.DirectoryName, file.Name, report.LoadSubreportDefinition, reportCorrectionMode);
            report.LoadReportDefinition(stream);
        }

        public static void LoadReport(this LocalReport report, Stream definition, ReportCorrectionMode reportCorrectionMode)
        {
            LoadReport(report, definition, "", reportCorrectionMode);
        }

        public static void LoadReport(this LocalReport report, Stream definition, string subReportSearchPath, ReportCorrectionMode reportCorrectionMode)
        {
            var xDoc = new XDocument();
            xDoc = XDocument.Load(definition);

            var stream = LoadReportDefinition(xDoc, subReportSearchPath, report.LoadSubreportDefinition, reportCorrectionMode);
            report.LoadReportDefinition(stream);
        }

        private static MemoryStream LoadReportDefinition(string reportPath, string reportFileName, Action<string, Stream> subReportLoader, ReportCorrectionMode reportCorrectionMode)
        {
            var xDoc = new XDocument();
            xDoc = XDocument.Load(Path.Combine(reportPath, reportFileName));

            return LoadReportDefinition(xDoc, reportPath, subReportLoader, reportCorrectionMode);
        }

        private static MemoryStream LoadReportDefinition(XDocument xDoc, string subReportPath, Action<string, Stream> subReportLoader, ReportCorrectionMode reportCorrectionMode)
        {
            bool ignoreGlobalVariables = true;
            bool convertSlashBetweenDigitsToDecimalSepratorParameter =true;

            var stream = new MemoryStream();

            XNamespace ns = xDoc.Root.Name.Namespace;
            XNamespace rdNs = xDoc.Root.Attributes().Where(o => o.Name.LocalName == "rd").First().Value;

            //1st: handle sub reports
            foreach (var subReport in xDoc.Descendants(ns + "Subreport").Descendants(ns + "ReportName"))
            {
                string subReportFileName = subReport.Value + ".rdlc";
                subReportLoader(subReport.Value, LoadReportDefinition(subReportPath, subReportFileName, subReportLoader, reportCorrectionMode));
            }

            //2nd : do correction
            if (reportCorrectionMode == ReportCorrectionMode.HmFontsCorrection)
                PersianRenderer.CorrectHmFonts(xDoc, ignoreGlobalVariables, convertSlashBetweenDigitsToDecimalSepratorParameter);

            //4th: return processed file
            xDoc.Save(stream);
            xDoc = null;
            stream.Position = 0;//we should call this!! unless the report throw an exception!
            return stream;
        }

    }
}
