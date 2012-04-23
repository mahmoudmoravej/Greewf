using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace Greewf.BaseLibrary.MVC.ChangeTracker
{
    public interface ISavingTracker
    {
        Action<DbContext> OnSavingChanges { get; set; }
        Action<DbContext> OnSavedChanges { get; set; }
    }
}
