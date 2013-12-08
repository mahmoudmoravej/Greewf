using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using Greewf.BaseLibrary.MVC.ChangeTracker.ChangeTrackerContext;
using System.Data.Entity;
using System.Data;

namespace Greewf.BaseLibrary.MVC.ChangeTracker
{
    public class AuditingWidget
    {
        public AuditingWidget()
        {
            TrackedEntities = new Dictionary<ObjectStateEntry,AuditLog>();
            TrackedEntityDetails = new Dictionary<AuditLogDetail, EntityKey>();
        }

        internal Dictionary<ObjectStateEntry, AuditLog> TrackedEntities { get; private set; }
        internal Dictionary<AuditLogDetail, EntityKey> TrackedEntityDetails { get; private set; }

        internal DbContext TrackedContext { get; set; }
        internal ChangeTrackerContext.ChangeTrackerContext ChangeTrackerContext { get; set; }

        internal DateTime AuditingDateTime { get; set; }
    }
}
