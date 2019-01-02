using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.IO;

namespace Greewf.Reporting
{
    public class ReportPersianRendererTask : Task
    {
        [Required]
        public string ReportDefinitionFileName { get; set; }

        [Required]
        public string OutputFileName { get; set; }

        public bool IgnoreGlobalVariables { get; set; } = true;

        public bool ConvertSlashBetweenDigitsToDecimalSepratorParameter { get; set; } = true;

        public override bool Execute()
        {
            ReportDefinitionFileName = Path.GetFullPath(ReportDefinitionFileName);
            OutputFileName = Path.GetFullPath(OutputFileName);

            Log.LogMessage(MessageImportance.Normal, "Correcting report path is: \"" + ReportDefinitionFileName + "\"");
            Log.LogMessage(MessageImportance.Normal, "Corrected report will be saved on: \"" + OutputFileName + "\"");

            try
            {
                PersianRenderer.CorrectReportDefinition(ReportDefinitionFileName, OutputFileName, IgnoreGlobalVariables, ConvertSlashBetweenDigitsToDecimalSepratorParameter);
                Log.LogMessage("Corrected Report file saved successfully.");
                return true;
            }
            catch (System.Exception x)
            {

                Log.LogErrorFromException(new System.Exception($"An error occured during the execution of {nameof(ReportPersianRendererTask)} task. The requested report file is \"{ReportDefinitionFileName}\" and the requested outputfile is \"{OutputFileName}\". The inner exception is: {x.ToString()}", x));
                return false;
            }
        }
    }
}
