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

        /// <summary>
        /// this event is intended to be raised after saving and before transaction scope commission
        /// NOTE: if there is no transaction scope, this event will be same with OnChangesCommitted
        /// </summary>
        event Action<DbContext> OnChangesSaved;


        /// <summary>
        /// this event is intended to be raised after saving and after transaction scope commission
        /// NOTE: if there is no transaction scope, this event will be same with OnChangesSaved
        /// </summary>
        event Action OnChangesCommitted;

        /// <summary>
        /// this event is intended to be raised before starting transaction scope
        /// NOTE: if there is no transaction scope, this event will be same with OnChangesSaving
        /// </summary>
        event Action OnBeforeTransactionStart;

    }

}
