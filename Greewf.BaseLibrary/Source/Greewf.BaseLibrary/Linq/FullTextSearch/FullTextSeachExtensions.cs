//Source : http://stackoverflow.com/a/26762756/790811

using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Greewf.BaseLibrary.Linq
{
    public static class FullTextSearchExtensions
    {

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="expression"></param>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> FreeTextSearch<TEntity>(this IQueryable<TEntity> source, Expression<Func<TEntity, object>> expression, string searchTerm) where TEntity : class
        {
            return effts.FullTextSearchExtensions.FreeTextSearch(source, expression, searchTerm);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="expression"></param>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> ContainsSearch<TEntity>(this IQueryable<TEntity> source, Expression<Func<TEntity, object>> expression, string searchTerm) where TEntity : class
        {
            return effts.FullTextSearchExtensions.ContainsSearch(source, expression, searchTerm);
        }

    }
}
