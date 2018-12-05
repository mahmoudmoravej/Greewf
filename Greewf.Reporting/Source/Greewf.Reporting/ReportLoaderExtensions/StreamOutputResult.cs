using System.Collections.Generic;
using System.IO;
using Microsoft.Reporting.WebForms;


namespace Greewf.Reporting
{
    public class StreamOutputResult
    {
        public List<Stream> Streams { get; set; }

        public Warning[] Warnings { get; set; }
    }
}
