using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Reporting.WebForms;


namespace Greewf.BaseLibrary.ReportLoaderExtensions
{
    public class StreamOutputResult
    {
        public List<Stream> Streams { get; set; }

        public Warning[] Warnings { get; set; }
    }
}
