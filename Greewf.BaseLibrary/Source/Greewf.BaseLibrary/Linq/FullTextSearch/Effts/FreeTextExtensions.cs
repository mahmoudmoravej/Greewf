using System;
using System.Linq;
using System.Linq.Expressions;

namespace effts
{
    /// <summary>
    /// 
    /// </summary>
    public static class FullTextSearchExtensions
    {

        /// <summary>
        /// این تابع متن جستجوی خام را قبول می کند و وظیفه اماده کردن محتوای بدون ایران محتوا بر عهده کاربر است
        /// بطور مثال رعایت کردن کوتیشن ها
        /// این باز بودن کمک می کند که بتوان ترکیب های مختلفی از تابع را نوشتو. بطور مثال از کلمه کلیدی "اند" استفاده کرد
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="expression"></param>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> FullTextFreeText<TEntity>(this IQueryable<TEntity> source, Expression<Func<TEntity, object>> expression, string searchTerm) where TEntity : class
        {
            return FreeTextSearchImp(source, expression, FullTextPrefixes.Freetext(searchTerm, false));//در این حالت اجازه ورود اند را می دهیم 
        }


        /// <summary>
        /// این تابع متن جستجوی خام را قبول می کند و وظیفه اماده کردن محتوای بدون ایران محتوا بر عهده کاربر است
        /// بطور مثال رعایت کردن کوتیشن ها
        /// این باز بودن کمک می کند که بتوان ترکیب های مختلفی از تابع را نوشتو. بطور مثال از کلمه کلیدی "اند" استفاده کرد
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="expression"></param>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> FullTextContains<TEntity>(this IQueryable<TEntity> source, Expression<Func<TEntity, object>> expression, string searchTerm) where TEntity : class
        {
            return FreeTextSearchImp(source, expression, FullTextPrefixes.Contains(searchTerm, false));//در این حالت اجازه ورود اند را می دهیم 
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="expression"></param>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        private static IQueryable<TEntity> FreeTextSearchImp<TEntity>(this IQueryable<TEntity> source, Expression<Func<TEntity, object>> expression, string searchTerm)
        {
            if (String.IsNullOrEmpty(searchTerm))
            {
                return source;
            }

            // The below represents the following lamda:
            // source.Where(x => x.[property].Contains(searchTerm))

            //Create expression to represent x.[property].Contains(searchTerm)
            //var searchTermExpression = Expression.Constant(searchTerm);
            var searchTermExpression = Expression.Property(Expression.Constant(new { Value = searchTerm }), "Value");
            var checkContainsExpression = Expression.Call(expression.Body, typeof(string).GetMethod("Contains"), searchTermExpression);

            //Join not null and contains expressions

            var methodCallExpression = Expression.Call(typeof(Queryable),
                                                       "Where",
                                                       new[] { source.ElementType },
                                                       source.Expression,
                                                       Expression.Lambda<Func<TEntity, bool>>(checkContainsExpression, expression.Parameters));

            return source.Provider.CreateQuery<TEntity>(methodCallExpression);
        }

    }
}
