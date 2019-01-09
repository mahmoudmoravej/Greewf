using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Greewf.Reporting
{

    public class PersianRenderer
    {
        public const string GreewfIgnorePersianCorrectionParameterName = "GreewfIgnorePersianCorrection";

        public static void CorrectReportDefinition(string reportDefinitionFileName, string outputFileName, bool ignoreGlobalVariables = true, bool convertSlashBetweenDigitsToDecimalSepratorParameter = true)
        {
            var xDoc = new XDocument();
            xDoc = XDocument.Load(reportDefinitionFileName);

            CorrectHmFonts(xDoc, ignoreGlobalVariables, convertSlashBetweenDigitsToDecimalSepratorParameter);

            //generate path if it doesn't exist
            var path = Path.GetDirectoryName(outputFileName);
            Directory.CreateDirectory(path);

            xDoc.Save(outputFileName);
            xDoc = null;
        }

        public static void CorrectHmFonts(XDocument xDoc, bool ignoreGlobalVariables = true, bool convertSlashBetweenDigitsToDecimalSepratorParameter = true)
        {
            XNamespace ns = xDoc.Root.Name.Namespace;
            var strConvertSlashBetweenDigitsToDecimalSepratorParameter = convertSlashBetweenDigitsToDecimalSepratorParameter ? "true" : "false";

            //1: handle greewf switches
            foreach (var prop in xDoc.Root.Elements(ns + "CustomProperties").Descendants(ns + "CustomProperty"))
            {

                var nameNode = prop.Descendants(ns + "Name").FirstOrDefault();
                if (nameNode != null && nameNode.Value == "GreewfIgnoreGlobalVariablesAtStart" && prop.Descendants(ns + "Value").Any(o => (o.Value ?? "").Trim().ToLower() == "false"))
                    ignoreGlobalVariables = false;
                else if (nameNode != null && nameNode.Value == "GreewfConvertSlashBetweenDigitsToDecimalSeprator" && prop.Descendants(ns + "Value").Any(o => (o.Value ?? "").Trim().ToLower() == "false"))
                    strConvertSlashBetweenDigitsToDecimalSepratorParameter = "false";
            }

            //2: add custom parameters
            AddCorrectionModelParameter(xDoc, ns);

            //3: process rdlc definition file
            ProcessTextRuns(xDoc, ignoreGlobalVariables, strConvertSlashBetweenDigitsToDecimalSepratorParameter, ns);
            ProcessCharts(xDoc, ignoreGlobalVariables, strConvertSlashBetweenDigitsToDecimalSepratorParameter, ns);
        }

        private static void AddCorrectionModelParameter(XDocument xDoc, XNamespace ns)
        {

            var parameters = xDoc.Root.Elements(ns + "ReportParameters").FirstOrDefault();
            if (parameters == null)
            {
                xDoc.Root.Add(new XElement(ns + "ReportParameters"));
                parameters = xDoc.Root.Elements(ns + "ReportParameters").FirstOrDefault();
            }

            parameters.Add(XElement.Parse(//توجه! وجود تگ پرامت الزامی است چراکه در غیر اینصورت آنرا یک پارامتر داخلی می شناسد و اجازه ارسال داده به آنرا نمی دهد
                $@"
                <ReportParameter Name=""{GreewfIgnorePersianCorrectionParameterName}"" xmlns=""{ns}"">
                    <DataType>Boolean</DataType>
                    <DefaultValue>
                        <Values>
                            <Value>false</Value>
                        </Values>
                    </DefaultValue>
                    <Prompt>""{GreewfIgnorePersianCorrectionParameterName}""</Prompt>
                    <Hidden>true</Hidden>
                </ReportParameter>
            "));

            //correct the parameter layout (unfortunately, we need to correct this part either)
            var layout = xDoc.Root.Elements(ns + "ReportParametersLayout").FirstOrDefault();
            if (layout == null)
            {
                xDoc.Root.Add(XElement.Parse(
                    $@"<ReportParametersLayout xmlns=""{ns}"">
                          <GridLayoutDefinition>
                            <NumberOfColumns>2</NumberOfColumns>
                            <NumberOfRows>1</NumberOfRows>
                            <CellDefinitions>
                            </CellDefinitions>
                          </GridLayoutDefinition>
                      </ReportParametersLayout>
                    "
                   )
                 );

                layout = xDoc.Root.Elements(ns + "ReportParametersLayout").FirstOrDefault();
            }

            var gridLayout = layout.Descendants(ns + "GridLayoutDefinition").FirstOrDefault();

            int numberOfRows = 0;
            var numberOfRowsNode = gridLayout.Descendants(ns + "NumberOfRows").FirstOrDefault();
            numberOfRowsNode.SetValue(numberOfRows = int.Parse(numberOfRowsNode.Value) + 1);

            var cellDefinitions = gridLayout.Descendants(ns + "CellDefinitions").FirstOrDefault();
            if (cellDefinitions == null)
            {
                gridLayout.Add(XElement.Parse(
                       $@"<CellDefinitions xmlns=""{ns}"">            
                          </CellDefinitions>
                        "
                ));

                cellDefinitions = gridLayout.Descendants(ns + "CellDefinitions").FirstOrDefault();
            }

            cellDefinitions.Add(XElement.Parse($@"
                <CellDefinition xmlns=""{ns}"">
                  <ColumnIndex>0</ColumnIndex>
                  <RowIndex>{numberOfRows - 1}</RowIndex>
                  <ParameterName>{GreewfIgnorePersianCorrectionParameterName}</ParameterName>
                </CellDefinition>
            "));

            //add it to subreports too
            foreach (var subReport in xDoc.Descendants(ns + "Subreport"))
            {
                var subReportParameters = subReport.Descendants(ns + "Parameters").FirstOrDefault();
                if (subReportParameters == null)
                {
                    subReport.Add(XElement.Parse(
                           $@"<Parameters xmlns=""{ns}"">            
                          </Parameters>
                        "
                    ));

                    subReportParameters = subReport.Descendants(ns + "Parameters").FirstOrDefault();
                }


                subReportParameters.Add(XElement.Parse($@"
                <Parameter Name=""{GreewfIgnorePersianCorrectionParameterName}""  xmlns=""{ns}"">
                  <Value>=Parameters!{GreewfIgnorePersianCorrectionParameterName}.Value</Value>
                </Parameter>
            "));

            }
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

                //correct X : like what we have in ChartSeriesHierarchy > ChartData > ChartSeriesCollection > ChartSeries > ChartDataPoints > ChartDataPoint > ChartDataPointValues > X (in xml definition)
                foreach (var label in chart.Descendants(ns + "ChartDataPointValues").Descendants(ns + "X"))
                {
                    CorrectValueNode(label, null, ignoreGlobalVariables, convertSlashBetweenDigitsToDecimalSepratorParameter);
                }


            }
        }

        private static void CorrectValueNode(XElement valueNode, XElement formatNode, bool ignoreGlobalVariables, string convertSlashBetweenDigitsToDecimalSepratorParameter)
        {
            if (valueNode.Value.TrimStart(' ', '\n', '\r').StartsWith("="))
            {
                if (ignoreGlobalVariables && valueNode.Value.TrimStart(' ', '\n', '\r', '=').StartsWith("globals!", true, null))
                    valueNode.Value = valueNode.Value;

                else
                {

                    string format = "nothing";
                    if (formatNode != null) format = "\"" + formatNode.Value + "\"";

                    valueNode.Value =
                        $"=Greewf.Reporting.Global.HmxFontCorrectorExceptExcel({ valueNode.Value.TrimStart(' ', '\n', '\r', '=')},Globals!RenderFormat.Name,{format},{convertSlashBetweenDigitsToDecimalSepratorParameter},Parameters!{GreewfIgnorePersianCorrectionParameterName}.Value)";
                }
            }
            else if (!string.IsNullOrWhiteSpace(valueNode.Value)) //constant string except white spaces
            {
                var newValue = valueNode.Value.Replace("\"", "\"\"").Replace("\r\n", "\" + vbCrlf + \"");
                valueNode.Value = $"=Greewf.Reporting.Global.HmxFontCorrectorExceptExcel(\"{newValue}\",Globals!RenderFormat.Name,nothing,{convertSlashBetweenDigitsToDecimalSepratorParameter},Parameters!{GreewfIgnorePersianCorrectionParameterName}.Value)";
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
