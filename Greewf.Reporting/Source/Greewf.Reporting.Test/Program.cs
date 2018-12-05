using Microsoft.Reporting.WebForms;
using System;
using System.IO;

namespace Greewf.Reporting.Test
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Genrating HM Fonts corrected report....");

            //try
            //{
            var localReport = new LocalReport();

            var repPath = "..\\..\\SampleReport.rdlc";
            localReport.LoadReport(repPath, ReportCorrectionMode.HmFontsCorrection);

            var report = ReportsLoader.ExportToFile(localReport, new ReportSettings() { OutputType = ReportingServiceOutputFileFormat.PDF });

            File.WriteAllBytes("..\\..\\SampleResult.pdf", report);

            Console.WriteLine("Report Generated!");

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
