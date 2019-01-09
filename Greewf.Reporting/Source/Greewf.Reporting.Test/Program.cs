using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Greewf.Reporting.Test
{
    class Program : IDisposable
    {
        private List<Process> processes = new List<Process>();

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

            using (var program = new Program())
            {
                program.Start();
            }

        }

        private void Start()
        {


            var xml = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).GetSection("runtime").SectionInformation.GetRawXml();
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            bool isCasMode = doc.GetElementsByTagName("legacyCasPolicy")?[0]?.Attributes?["enabled"]?.Value == "true";

            ReportsLoader.RunInLegacyCasModel = isCasMode;//اگر این مقدار را  تغییر دادید مقدار معادل آنرا در کانفیگ هم تغییر دهید

            Console.WriteLine("Test mode : " + (ReportsLoader.RunInLegacyCasModel ? "Legacy CAS Mode(Superfast reports)" : "Regular .Net 4.0 Settings(super slow reports!)"));
            Console.WriteLine("Genrating HM Fonts corrected report....");



            var reportFileName = "SampleReport";
            var subReportFileName = "SampleSubReport";

            var reportPath = $"..\\..\\{reportFileName}.rdlc";
            var subReportPath = $"..\\..\\{subReportFileName}.rdlc";

            //1st test : correct the report definition by MsBuild Task!
            Test01();

            //2nd test : correct report on demand (usable for rdlc files (or client report files)
            Test02(reportPath, processes);

            //3rd test: correct file externally (usable for rdl files (reporting service files where we should correct them at build time by a msbuild task)
            Test03(reportPath, subReportPath, processes);

            //4th test : correct report on demand but ignore persianc correction by a parameter (useful for rdl)
            Test04(reportPath, processes);

            Console.WriteLine("Press any key to finish...");
            Console.Read();


        }


        private static void Test01()
        {
            //NOTE!!!!!! this task will be called at build time. so if you get any error at that time, you should check the Output window to find out the source of the problem
            Console.WriteLine($"Report {nameof(Test01)} Generated at build time! (it will not be NOT opend. just check it manullay...)");
        }


        private static void Test02(string repPath, List<Process> processes)
        {

            var outputfile = $"..\\..\\TestResults\\{nameof(Test02)}.OnDemandRendering.pdf";
            var localReport = new LocalReport();

            localReport.ShowDetailedSubreportMessages = true;
            localReport.LoadReport(repPath, ReportCorrectionMode.HmFontsCorrection);

            var report = ReportsLoader.ExportToFile(localReport, new ReportSettings() { OutputType = ReportingServiceOutputFileFormat.PDF });

            File.WriteAllBytes(outputfile, report);

            Console.WriteLine($"Report {nameof(Test02)} Generated! (and will be opened in a second...)");
            processes.Add(System.Diagnostics.Process.Start(outputfile));

        }

        private static void Test03(string inputReportFile, string inputSubReportFile, List<Process> processes)
        {
            var outputReportFileBaseName = $"..\\..\\TestResults\\{nameof(Test03)}.PersianRenderer";

            var correctedFileDefinition = outputReportFileBaseName + ".rdlc";
            var correctedSubReportFileDefinition = outputReportFileBaseName + ".SubReport.rdlc";
            var correctedFileOutput = outputReportFileBaseName + ".pdf";


            PersianRenderer.CorrectReportDefinition(inputReportFile, correctedFileDefinition);
            PersianRenderer.CorrectReportDefinition(inputSubReportFile, correctedSubReportFileDefinition);

            var localReport = new LocalReport();
            localReport.ShowDetailedSubreportMessages = true;

            localReport.LoadReport(correctedFileDefinition, ReportCorrectionMode.None);

            var textReader = File.OpenText(correctedSubReportFileDefinition);
            localReport.LoadSubreportDefinition("Subreport1", textReader);          //I dont' know why it doesn't work  

            var report = ReportsLoader.ExportToFile(localReport, new ReportSettings() { OutputType = ReportingServiceOutputFileFormat.PDF });

            File.WriteAllBytes(correctedFileOutput, report);

            Console.WriteLine($"Report {nameof(Test03)} Generated! (and will be opened in a second...)");
            processes.Add(System.Diagnostics.Process.Start(correctedFileOutput));
        }

        private static void Test04(string repPath, List<Process> processes)
        {
            //try
            //{

            var outputfile = $"..\\..\\TestResults\\{nameof(Test04)}.OnDemandRenderingButIgnoreCorrection.pdf";
            var localReport = new LocalReport();
            localReport.ShowDetailedSubreportMessages = true;

            localReport.LoadReport(repPath, ReportCorrectionMode.HmFontsCorrection);

            var report = ReportsLoader.ExportToFile(localReport, new ReportSettings() { OutputType = ReportingServiceOutputFileFormat.PDF, IgnorePersianCorrection = true });

            File.WriteAllBytes(outputfile, report);

            Console.WriteLine($"Report {nameof(Test04)} Generated! (and will be opened in a second...)");
            processes.Add(System.Diagnostics.Process.Start(outputfile));

            //}
            //catch (Exception x)
            //{
            //    throw;
            //}
        }

        public void Dispose()
        {
            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                    process.WaitForExit();
                }
                catch
                {

                }

            }
        }

    }
}
