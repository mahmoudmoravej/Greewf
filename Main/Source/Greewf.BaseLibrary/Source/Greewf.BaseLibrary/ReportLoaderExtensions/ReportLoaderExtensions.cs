using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Greewf.BaseLibrary.ReportLoaderExtensions
{

    public enum ReportCorrectionMode
    {
        None = 0,
        HmFontsCorrection = 1,
    }

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

            var stream = LoadReportDefinition(file.DirectoryName, file.Name, report.LoadSubreportDefinition);
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

            var stream = LoadReportDefinition(xDoc, subReportSearchPath, report.LoadSubreportDefinition);
            report.LoadReportDefinition(stream);
        }

        private static MemoryStream LoadReportDefinition(string reportPath, string reportFileName, Action<string, Stream> subReportLoader)
        {
            var xDoc = new XDocument();
            xDoc = XDocument.Load(Path.Combine(reportPath, reportFileName));

            return LoadReportDefinition(xDoc, reportPath, subReportLoader);
        }

        private static MemoryStream LoadReportDefinition(XDocument xDoc, string subReportPath, Action<string, Stream> subReportLoader)
        {
            bool ignoreGlobalVariables = true;
            string convertSlashBetweenDigitsToDecimalSepratorParameter = "true";

            var stream = new MemoryStream();

            XNamespace ns = xDoc.Root.Name.Namespace;
            XNamespace rdNs = xDoc.Root.Attributes().Where(o => o.Name.LocalName == "rd").First().Value;

            //1st: handle sub reports
            foreach (var subReport in xDoc.Descendants(ns + "Subreport").Descendants(ns + "ReportName"))
            {
                string subReportFileName = subReport.Value + ".rdlc";
                subReportLoader(subReport.Value, LoadReportDefinition(subReportPath, subReportFileName, subReportLoader));
            }

            //2nd : handle greewf switches
            foreach (var prop in xDoc.Root.Elements(ns + "CustomProperties").Descendants(ns + "CustomProperty"))
            {

                var nameNode = prop.Descendants(ns + "Name").FirstOrDefault();
                if (nameNode != null && nameNode.Value == "GreewfIgnoreGlobalVariablesAtStart" && prop.Descendants(ns + "Value").Any(o => (o.Value ?? "").Trim().ToLower() == "false"))
                    ignoreGlobalVariables = false;
                else if (nameNode != null && nameNode.Value == "GreewfConvertSlashBetweenDigitsToDecimalSeprator" && prop.Descendants(ns + "Value").Any(o => (o.Value ?? "").Trim().ToLower() == "false"))
                    convertSlashBetweenDigitsToDecimalSepratorParameter = "false";
            }


            //3rd : process rdlc definition file
            ProcessTextRuns(xDoc, ignoreGlobalVariables, convertSlashBetweenDigitsToDecimalSepratorParameter, ns);
            ProcessCharts(xDoc, ignoreGlobalVariables, convertSlashBetweenDigitsToDecimalSepratorParameter, ns);

            //4th: return processed file
            xDoc.Save(stream);
            xDoc = null;
            stream.Position = 0;//we should call this!! unless the report throw an exception!
            return stream;
        }

        private static void ProcessTextRuns(XDocument xDoc, bool ignoreGlobalVariables, string convertSlashBetweenDigitsToDecimalSepratorParameter, XNamespace ns)
        {
            foreach (var textRun in xDoc.Descendants(ns + "TextRun"))
            {

                var textRunStyle = textRun.Descendants(ns + "Style").FirstOrDefault();
                if (textRunStyle != null && textRunStyle.Descendants(ns + "FontFamily").Any(o => o.Value.StartsWith("hm ", true, null)))
                {
                    var parentTextBox = textRun.Ancestors(ns + "Textbox").FirstOrDefault();
                    if (IgnoreThisNodeCorrection(parentTextBox)) continue;

                    var textRunValue = textRun.Element(ns + "Value");
                    var textRunFormat = textRunStyle.Descendants(ns + "Format").FirstOrDefault();

                    CorrectValueNode(textRunValue, textRunFormat, ignoreGlobalVariables, convertSlashBetweenDigitsToDecimalSepratorParameter);
                }

            }
        }

        private static void ProcessCharts(XDocument xDoc, bool ignoreGlobalVariables, string convertSlashBetweenDigitsToDecimalSepratorParameter, XNamespace ns)
        {
            //we assume if a grid has a hm font, we should correct all labels inside it
            foreach (var chart in
                xDoc.Descendants(ns + "Chart")
                .Where(o => o.Descendants(ns + "FontFamily").Any(b => b.Value.StartsWith("hm ", true, null))))
            {

                if (IgnoreThisNodeCorrection(chart)) continue;

                //correct labels : like what we have in ChartCategoryHierarchy > ChartMembers > ChartMember > Label (in xml definition)
                foreach (var label in chart.Descendants(ns + "Label"))
                {
                    CorrectValueNode(label, null, ignoreGlobalVariables, convertSlashBetweenDigitsToDecimalSepratorParameter);
                }

                //correct X and Y : like what we have in ChartSeriesHierarchy > ChartData > ChartSeriesCollection > ChartSeries > ChartDataPoints > ChartDataPoint > ChartDataPointValues > X (in xml definition)
                foreach (var label in chart.Descendants(ns + "ChartDataPointValues").Descendants(ns + "X"))
                {
                    CorrectValueNode(label, null, ignoreGlobalVariables, convertSlashBetweenDigitsToDecimalSepratorParameter);
                }


            }
        }

        private static void CorrectValueNode(XElement valueNode, XElement formatNode, bool ignoreGlobalVariables, string convertSlashBetweenDigitsToDecimalSepratorParameter)
        {
            if (valueNode.Value.TrimStart(' ').StartsWith("="))
            {
                if (ignoreGlobalVariables && valueNode.Value.TrimStart(' ', '=').StartsWith("globals!", true, null))
                    valueNode.Value = valueNode.Value;

                else
                {

                    string format = "nothing";
                    if (formatNode != null) format = "\"" + formatNode.Value + "\"";

                    valueNode.Value =
                        "=Greewf.BaseLibrary.Global.HmxFontCorrectorExceptExcel(" +
                        valueNode.Value.TrimStart(' ', '=') +
                        ",Globals!RenderFormat.Name," + format + "," + convertSlashBetweenDigitsToDecimalSepratorParameter + ")";
                }
            }
            else if (!string.IsNullOrWhiteSpace(valueNode.Value)) //constant string except white spaces
            {
                var newValue = valueNode.Value.Replace("\"", "\"\"").Replace("\r\n", "\" + vbCrlf + \"");
                valueNode.Value = "=Greewf.BaseLibrary.Global.HmxFontCorrectorExceptExcel(\"" + newValue + "\",Globals!RenderFormat.Name,nothing," + convertSlashBetweenDigitsToDecimalSepratorParameter + ")";
            }
        }

        private static bool IgnoreThisNodeCorrection(XElement node)
        {
            XNamespace ns = node.Name.Namespace;

            if (node != null)
            {
                node = node.Descendants(ns + "CustomProperty").Where(o => o.Descendants(ns + "Name").First().Value == "GreewfIgnoreCorrection").LastOrDefault();
                if (node != null && (node.Descendants(ns + "Value").First().Value ?? "").ToLower() == "true")
                    return true;
            }

            return false;
        }
    }
}
