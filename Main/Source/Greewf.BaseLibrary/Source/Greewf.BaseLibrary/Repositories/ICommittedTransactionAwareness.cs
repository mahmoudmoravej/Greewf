using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Transactions;

namespace Greewf.BaseLibrary.Repositories
{
 
    /// <summary>
    /// منظور امکان آگاهی یافتن از زمانی است که تراکنش بصورت کامل کامیت شده است
    /// یا به بیان دیگر
    /// persist 
    /// شده است
    /// </summary>
    public interface ITransactionScopeAwareness
    {
        event Action OnChangesCommitted;


        /// <summary>
        /// در صورت خالی بودن یعنی اسکوپی در کار نیست
        /// </summary>
        TransactionScope TransactionScope { get; set; }

        //از آنجا که ایونت را خارج از کلاس نمی توان فراخوانی کرد نیاز است که این تابع در کلاس پیاده سازی شود
        // تا کلاسی که مسول تراکنش بیرونی است در زمان کامیت شدن تراکنش، آنرا به
        //اطلاع کانتکست برساند
        void OnChangesCommittedEventInvoker();
    }
}
