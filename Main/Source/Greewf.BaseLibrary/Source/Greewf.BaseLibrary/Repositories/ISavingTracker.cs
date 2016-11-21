using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;

namespace Greewf.BaseLibrary.Repositories
{
    public interface ISavingTracker
    {
        event Action<DbContext> OnChangesSaving;
        event Action<DbContext> OnChangesSaved;
    }
 
}
