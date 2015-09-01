using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace Greewf.BaseLibrary.Logging.LogContext
{
    public partial class LogContext : DbContext
    {
        public LogContext(string connectionString)
            : base(connectionString)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
        }

    }
}
