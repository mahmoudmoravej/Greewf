using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Greewf.BaseLibrary.MVC.ChangeTracker.ChangeTrackerContext;
using System.Data.Entity;
using System.Linq.Expressions;

namespace Greewf.BaseLibrary.MVC.ChangeTracker
{

    public abstract class ChangeTrackerEntityDetailsProvider
    {

        public abstract RelationInfo GetRelatedEntity(object entity, DbContext context);

        /// <summary>
        /// we call this when the GetReatedEntity's Entity is null. (it is needed in deleted records when all assocications are removed)
        /// </summary>
        /// <param name="entitySet"></param>
        /// <param name="entityKey"></param>
        /// <returns></returns>
        public abstract object GetRelatedEntityByKey(object entitySet, object entityKey);
        public abstract string GetRelationName(string entityName, string parentEntityName);
        public abstract List<Y> CorrectToView<Y>(List<Y> auditDetails, DbContext context);

        public void RegisterForCurrentProvider(string entityName)
        {
            ChangeTrackerDetailsProvider.Current.AddEntityDetailsProvider(entityName, this);
        }

        protected ModelMetadata GetMetaDataFor<T>() where T : new()
        {
            T o = new T();
            return ModelMetadataProviders.Current.GetMetadataForType(() => o, typeof(T));

        }

        protected string GetPropertyName<TValue>(Expression<Func<TValue>> expression)
        {
            return (expression.Body as MemberExpression).Member.Name;
        }

    }

    public class DefaultChangeTrackerEntityDetailsProvider : ChangeTrackerEntityDetailsProvider
    {


        public override string GetRelationName(string entityName, string parentEntityName)
        {
            return entityName;
        }

        public override List<Y> CorrectToView<Y>(List<Y> auditDetails, DbContext context)
        {
            return auditDetails;
        }

        public override RelationInfo GetRelatedEntity(object entity, DbContext context)
        {
            return null;
        }

        public override object GetRelatedEntityByKey(object entitySet,object entityKey)
        {
            return null;
        }
    }
}
