using Microsoft.Reporting.WebForms;
using System;
using System.Configuration;
using System.IO;
using System.Xml;

namespace Greewf.Reporting.Test
{
    class Program
    {
        static void Main(string[] args)
        {

            var xml = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).GetSection("runtime").SectionInformation.GetRawXml();
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            bool isCasMode = doc.GetElementsByTagName("legacyCasPolicy")?[0]?.Attributes?["enabled"]?.Value == "true";

            ReportsLoader.RunInLegacyCasModel = isCasMode;//اگر این مقدار را  تغییر دادید مقدار معادل آنرا در کانفیگ هم تغییر دهید

            Console.WriteLine("Test mode : " + (ReportsLoader.RunInLegacyCasModel ? "Legacy CAS Mode(Superfast reports)" : "Regular .Net 4.0 Settings(super slow reports!)"));
            Console.WriteLine("Genrating HM Fonts corrected report....");

            //try
            //{
            var localReport = new LocalReport();

            var repPath = "..\\..\\SampleReport.rdlc";
            localReport.LoadReport(repPath, ReportCorrectionMode.HmFontsCorrection);

            var report = ReportsLoader.ExportToFile(localReport, new ReportSettings() { OutputType = ReportingServiceOutputFileFormat.PDF });

            File.WriteAllBytes("..\\..\\SampleResult.pdf", report);

            Console.WriteLine("Report Generated! (and will be opened in a second...)");
            System.Diagnostics.Process.Start("..\\..\\SampleResult.pdf");

            //}
            //catch (Exception x)
            //{
            //    throw;
            //}

            Console.WriteLine("Press any key to finish...");
            Console.Read();

        }
    }
}
