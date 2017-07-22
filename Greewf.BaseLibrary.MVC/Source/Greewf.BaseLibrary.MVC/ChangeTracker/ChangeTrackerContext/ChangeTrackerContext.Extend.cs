using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace Greewf.BaseLibrary.MVC.ChangeTracker.ChangeTrackerContext
{
    public partial class ChangeTrackerContext : DbContext
    {
        public ChangeTrackerContext(string connectionString)
            : base(connectionString)
        {
            this.Configuration.AutoDetectChangesEnabled = false;
        }

    }
  
}
