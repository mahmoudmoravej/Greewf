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
            /*  !!!توجه!!!!! 
             *  بخشی از تست در زمان بیلد است چراکه یک تسک ام.اس.بیلد هم داریم
             *  لذا اگر خطا گرفتید باید آنرا برطرف کنید. 
             *  همچنین دقت کنید که به علت آنکه تسک  مربوطه در زمان بیلد هم ساخته می شود برخی مواقع برای اجرا مجدد
             *  لازم است که از تسک 
             *  msbuild
             *  مربوطه را 
             *  end task
             *  کنید و الا پیغام مشابه 
             *  Exceeded retry count of 10. Failed. The file is locked by:
             *  خواهید گرفت
             *  
             */



            var xml = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).GetSection("runtime").SectionInformation.GetRawXml();
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            bool isCasMode = doc.GetElementsByTagName("legacyCasPolicy")?[0]?.Attributes?["enabled"]?.Value == "true";

            ReportsLoader.RunInLegacyCasModel = isCasMode;//اگر این مقدار را  تغییر دادید مقدار معادل آنرا در کانفیگ هم تغییر دهید

            Console.WriteLine("Test mode : " + (ReportsLoader.RunInLegacyCasModel ? "Legacy CAS Mode(Superfast reports)" : "Regular .Net 4.0 Settings(super slow reports!)"));
            Console.WriteLine("Genrating HM Fonts corrected report....");



            var reportFileName = "SampleReport";
            var repPath = $"..\\..\\{reportFileName}.rdlc";

            //1st test : correct the report definition by MsBuild Task!
            Test01();

            //2nd test : correct report on demand (usable for rdlc files (or client report files)
            Test02(repPath);

            //3rd test: correct file externally (usable for rdl files (reporting service files where we should correct them at build time by a msbuild task)
            Test03(repPath);

            Console.WriteLine("Press any key to finish...");
            Console.Read();

        }

        private static void Test01()
        {
            //NOTE!!!!!! this task will be called at build time. so if you get any error at that time, you should check the Output window to find out the source of the problem
            Console.WriteLine($"Report {nameof(Test01)} Generated at build time! (it will not be NOT opend. just check it manullay...)");
        }


        private static void Test02(string repPath)
        {
            //try
            //{

            var outputfile = $"..\\..\\TestResults\\{nameof(Test02)}.OnDemandRendering.pdf";
            var localReport = new LocalReport();
            localReport.LoadReport(repPath, ReportCorrectionMode.HmFontsCorrection);

            var report = ReportsLoader.ExportToFile(localReport, new ReportSettings() { OutputType = ReportingServiceOutputFileFormat.PDF });

            File.WriteAllBytes(outputfile, report);

            Console.WriteLine($"Report {nameof(Test02)} Generated! (and will be opened in a second...)");
            System.Diagnostics.Process.Start(outputfile);

            //}
            //catch (Exception x)
            //{
            //    throw;
            //}
        }

        private static void Test03(string inputReportFile)
        {
            var outputReportFileBaseName = $"..\\..\\TestResults\\{nameof(Test03)}.PersianRenderer";
            var correctedFileDefinition = outputReportFileBaseName + ".rdlc";
            var correctedFileOutput = outputReportFileBaseName + ".pdf";

            PersianRenderer.CorrectReportDefinition(inputReportFile, correctedFileDefinition);

            var localReport = new LocalReport();
            localReport.LoadReport(correctedFileDefinition, ReportCorrectionMode.None);
            var report = ReportsLoader.ExportToFile(localReport, new ReportSettings() { OutputType = ReportingServiceOutputFileFormat.PDF });

            File.WriteAllBytes(correctedFileOutput, report);

            Console.WriteLine($"Report {nameof(Test03)} Generated! (and will be opened in a second...)");
            System.Diagnostics.Process.Start(correctedFileOutput);
        }

    }
}
