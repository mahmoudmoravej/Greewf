﻿using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Greewf.BaseLibrary.ReportLoaderExtentions
{

    public enum ReportCorrectionMode
    {
        None = 0,
        HmFontsCorrection = 1,
    }

    public static class LocalReportExtentions
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

        private static MemoryStream LoadReportDefinition(string reportPath, string reportFileName, Action<string, Stream> subReportLoader)
        {
            bool ignoreGlobalVariables = true;
            string convertSlashBetweenDigitsToDecimalSepratorParameter = "true";

            var xDoc = new XDocument();
            var stream = new MemoryStream();
            xDoc = XDocument.Load(Path.Combine(reportPath, reportFileName));

            XNamespace ns = xDoc.Root.Name.Namespace;
            XNamespace rdNs = xDoc.Root.Attributes().Where(o => o.Name.LocalName == "rd").First().Value;

            //1st: handle sub reports
            foreach (var subReport in xDoc.Descendants(ns + "Subreport").Descendants(ns + "ReportName"))
            {
                string subReportFileName = subReport.Value + ".rdlc";
                subReportLoader(subReport.Value, LoadReportDefinition(reportPath, subReportFileName, subReportLoader));
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
            foreach (var textRun in xDoc.Descendants(ns + "TextRun"))
            {

                var textRunStyle = textRun.Descendants(ns + "Style").FirstOrDefault();
                if (textRunStyle != null && textRunStyle.Descendants(ns + "FontFamily").Any(o => o.Value.StartsWith("hm ", true, null)))
                {

                    if (IgnoreThisNodeCorrection(textRun)) continue;

                    var textRunValue = textRun.Element(ns + "Value");

                    if (textRunValue.Value.TrimStart(' ').StartsWith("="))
                    {
                        if (ignoreGlobalVariables && textRunValue.Value.TrimStart(' ', '=').StartsWith("globals!", true, null))
                            textRunValue.Value = textRunValue.Value;

                        else
                        {
                            var textRunFormat = textRunStyle.Descendants(ns + "Format").FirstOrDefault();
                            string format = "nothing";
                            if (textRunFormat != null) format = "\"" + textRunFormat.Value + "\"";

                            textRunValue.Value =
                                "=Greewf.BaseLibrary.Global.HmxFontCorrectorExceptExcel(" +
                                textRunValue.Value.TrimStart(' ', '=') +
                                ",Globals!RenderFormat.Name," + format + "," + convertSlashBetweenDigitsToDecimalSepratorParameter + ")";
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(textRunValue.Value)) //constant string except white spaces
                    {
                        var newValue = textRunValue.Value.Replace("\"", "\"\"").Replace("\r\n", "\" + vbCrlf + \"");
                        textRunValue.Value = "=Greewf.BaseLibrary.Global.HmxFontCorrectorExceptExcel(\"" + newValue + "\",Globals!RenderFormat.Name,nothing," + convertSlashBetweenDigitsToDecimalSepratorParameter + ")";
                    }
                }

            }

            //4th: return processed file
            xDoc.Save(stream);
            xDoc = null;
            stream.Position = 0;//we should call this!! unless the report throw an exception!
            return stream;
        }

        private static bool IgnoreThisNodeCorrection(XElement textRun)
        {
            XNamespace ns = textRun.Name.Namespace;            
            var parentTextBox = textRun.Ancestors(ns + "Textbox").FirstOrDefault();

            if (parentTextBox != null)
            {
                var node = parentTextBox.Descendants(ns + "CustomProperty").Where(o => o.Descendants(ns + "Name").First().Value == "GreewfIgnoreCorrection").LastOrDefault();
                if (node != null && (node.Descendants(ns + "Value").First().Value ?? "").ToLower() == "true")
                    return true;
            }

            return false;
        }
    }
}
