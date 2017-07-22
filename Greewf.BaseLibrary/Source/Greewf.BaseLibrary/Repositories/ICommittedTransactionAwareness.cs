using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Transactions;

namespace Greewf.BaseLibrary.Repositories
{
 
    /// <summary>
    /// منظور امکان آگاهی یافتن از زمانی است که شی
    /// dbcontext
    /// در درون یک 
    /// Transactionscope 
    /// قرار گرفته است
    /// </summary>
    public interface ITransactionScopeAwareness
    {
        /// <summary>
        /// در صورت فالس بودن یعنی اسکوپی در کار نیست
        /// </summary>
        bool IsInActiveTransactionScope { get; set; }

        //از آنجا که ایونت را خارج از کلاس نمی توان فراخوانی کرد نیاز است که این تابع در کلاس پیاده سازی شود
        // تا کلاسی که مسول تراکنش بیرونی است در زمان کامیت شدن تراکنش، آنرا به
        //اطلاع کانتکست برساند
        void OnTransactionScopeCommitted();

        void OnTransactionScopeRollbacked();

        /// <summary>
        /// از آنجا که بعد از آنکه ترنزکشن شروع شد نمی توان کانکشن جدیدی را در آن شروع کرد مگر آنکه 
        /// سرویس تراکنش توزیع یافته یا 
        /// DTC
        /// شروع شده باشد، از طریق این تابع سعی می کنیم که در صورت امکان کانکشن های لازم را ایجاد کنیم
        /// </summary>
        void OnBeforeTransactionScopeStart();
    }
}
