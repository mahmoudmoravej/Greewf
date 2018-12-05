using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;


namespace Greewf.Reporting
{    
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
            _dicOutputTypes.Add(ReportingServiceOutputFileFormat.TIFF, "تصویر TIFF");
            _dicOutputTypes.Add(ReportingServiceOutputFileFormat.JPG, "تصویر JPEG");
            _dicOutputTypes.Add(ReportingServiceOutputFileFormat.EMF, "تصویر EMF");
        }

        private static string PrepareAndGetDeviceInfo(LocalReport report, ReportSettings settings)
        {

            /*
            برای افزایش سرعت در حالت سندباکس کارهای زیاد انجام دادیم ولی نشد. 
            توجه!!!!! خیلی جالب آنکه اجرا در لوکال از سرور های تست    
            خیلی سریعتیر است اما بعد از پابلیش در سرور همچنان کند است

            کارهای زیر انجام شد : 

            ----------------------------- کار 1 --------------------------------------------------------------

            کد زیر دقیقا در همین تابع و در همین ابندای آن قرار داده شد. کد کار می کند ولی تاثیری در سرعت نداشت : 
            
             
                PermissionSet permissions = new PermissionSet(PermissionState.Unrestricted);
                permissions.AddPermission(new SecurityPermission(PermissionState.Unrestricted));
                permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.AllFlags));
                permissions.AddPermission(new FileIOPermission(PermissionState.Unrestricted));
                permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));             

                report.SetBasePermissionsForSandboxAppDomain(permissions);

                var customAssembly = typeof(ReportsLoader).Assembly;// "Greewf.Reporting, Culture=neutral, PublicKeyToken=ebf2eb006a1f561b";
                StrongName assemblyStrongName = CreateStrongName(customAssembly);
                report.AddFullTrustModuleInSandboxAppDomain(assemblyStrongName);

            

            ----------------------------- کار 2 --------------------------------------------------------------

            همچنین تنظیمات زیر را هم در وب کانفیگ پروژه اجرایی گذاشتیم ولی باز تاثیری نداشت : 
            این موارد طبق این پست ها بود : https://stackoverflow.com/a/26733816/790811 & https://docs.microsoft.com/en-us/previous-versions/dotnet/netframework-4.0/ee191568(v=vs.100)
            
            در بخش configuration: 
        
              <startup uselegacyv2runtimeactivationpolicy="true">
              </startup> 

            و در بخش runtime: 

                <NetFx40_LegacySecurityPolicy enabled="true" />

            
            همچنین در اینجا یک پست هست که توضیح با بالاترین ووت هم خواندنی است : https://social.msdn.microsoft.com/Forums/sqlserver/en-US/6d89e2ce-3528-465f-9740-7e22aa7b7aae/slow-performance-with-dynamic-grouping-and-reportviewer-in-local-mode?forum=sqlreportingservices

            
            
            ----------------------------- کار 3 --------------------------------------------------------------
            در کانفیگ این کار را هم طبق پست روبرو انجام دادیم. سرعت به طرز وحشتناکی خوب شد : https://social.msdn.microsoft.com/Forums/en-US/b35bf409-4d73-4506-b13b-2629b1216773/reportviewer-in-net-4-even-possible-legacycasmodelquottruequot-causes-problems?forum=vsreportcontrols
            اما محدودیت آن فراوان است           
            
            در بخش system.web :            
                <trust legacyCasModel="true" level="Full"/>

            در تنظیمات اسمبلی همین اسمبلی باید خط زیر را اضافه کنید : https://stackoverflow.com/a/2504341/790811
            برای همین کار مجبور هستیم کل پروژه را جدا کنیم چراکه برای بخش های دیگر دچار مشکل می کند
                [assembly: AllowPartiallyTrustedCallers]

            و در کد دقیقا در همین محل عملیات زیر انجام شد : 
            var customAssembly = typeof(ReportsLoader).Assembly;
            report.AddTrustedCodeModuleInCurrentAppDomain(customAssembly.FullName);

            
            */

            //این کد در صورتی اثر می کند که در کانفیگ برنامه تنظیم گفته شده در توضیحات انجام شده باشد
            //توجه: اسمبلی را دستی نوشتیم. دقت کنید که اگر از خود اسمبلی می گرفتیم ورژن هم داشت و ما نباید
            //در این نام آنرا به ورژن وابسته کنیم
            report.AddTrustedCodeModuleInCurrentAppDomain("Greewf.Reporting, Culture=neutral, PublicKeyToken=ebf2eb006a1f561b");


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
            var deviceInfo = PrepareAndGetDeviceInfo(report, settings);
            return report.Render(GetOutputFileFormat(settings.OutputType), deviceInfo);

        }

        [Obsolete("Currently this method is unusable. PDF rendering needs seekable stream and EMF rendering not works with PushStreamContent.")]
        public static void PushToStream(LocalReport report, ReportSettings settings, Stream outputStream)
        {
            var deviceInfo = PrepareAndGetDeviceInfo(report, settings);

            Warning[] warnings;

            report.Render(GetOutputFileFormat(settings.OutputType), deviceInfo, PageCountMode.Estimate,
                (string name, string fileNameExtension, Encoding encoding, string mimeType, bool willSeek) =>
                {
                    if (willSeek == true && outputStream.CanSeek == false)
                        throw new Exception("Greewf : Current output report needs a seekable stream but you passed a not seekable stream. Change your output stream to a seekable one or change the report output type to one which doesn't need seekable stream. ");

                    return outputStream;
                },
                out warnings);

        }

        public static StreamOutputResult ExportToStreams(LocalReport report, ReportSettings settings)
        {
            var deviceInfo = PrepareAndGetDeviceInfo(report, settings);

            Warning[] warnings;
            var streams = new List<Stream>();

            report.Render(GetOutputFileFormat(settings.OutputType), deviceInfo, PageCountMode.Estimate,
                (string name, string fileNameExtension, Encoding encoding, string mimeType, bool willSeek) =>
                {
                    Stream stream = new MemoryStream();


                    streams.Add(stream);
                    return stream;
                },
                out warnings);

            foreach (Stream stream in streams)
                stream.Position = 0;

            //NOTE 1: only emf file returns multiple stream
            //NOTE 2: the streams cannot concatinat simply to make a large stream.  
            return new StreamOutputResult()
            {
                Streams = streams,
                Warnings = warnings
            };
        }


        private static string GetOutputFileFormat(ReportingServiceOutputFileFormat fileFormat)
        {
            //NOTE : get supported types by calling localReport.ListRenderingExtensions() . LocalReport does not support HTML output.
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
                case ReportingServiceOutputFileFormat.TIFF:
                case ReportingServiceOutputFileFormat.JPG:
                case ReportingServiceOutputFileFormat.EMF:
                    return "IMAGE";
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
                case ReportingServiceOutputFileFormat.TIFF:
                    return "image/tiff";
                case ReportingServiceOutputFileFormat.JPG:
                    return "image/jpeg";
                case ReportingServiceOutputFileFormat.EMF:
                    return "application/emf";
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
                case ReportingServiceOutputFileFormat.TIFF:
                    return "tiff";
                case ReportingServiceOutputFileFormat.JPG:
                    return "jpg";
                case ReportingServiceOutputFileFormat.EMF:
                    return "emf";
                default:
                    return "";
            }
        }

        public static string GetOutputFormatTitle(ReportingServiceOutputFileFormat format)
        {
            return _dicOutputTypes[format];
        }

        private static StrongName CreateStrongName(Assembly assembly)
        {
            AssemblyName assemblyName = assembly.GetName();
            if (assemblyName == null)
            {
                throw new InvalidOperationException("Could not get assemmbly name");
            }
            byte[] publickey = assemblyName.GetPublicKey();
            if (publickey == null || publickey.Length == 0)
            {
                throw new InvalidOperationException("Assembly is not strongly named");
            }
            StrongNamePublicKeyBlob keyblob = new StrongNamePublicKeyBlob(publickey);
            return new StrongName(keyblob, assemblyName.Name, assemblyName.Version);

        }


    }
}