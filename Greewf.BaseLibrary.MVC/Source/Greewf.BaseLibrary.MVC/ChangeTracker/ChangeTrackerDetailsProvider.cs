using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Greewf.BaseLibrary.MVC.ChangeTracker.ChangeTrackerContext;
using System.Data.Entity;

namespace Greewf.BaseLibrary.MVC.ChangeTracker
{
    public class ChangeTrackerDetailsProvider
    {

        private ChangeTrackerDetailsProvider()
        {

        }

        private static ChangeTrackerDetailsProvider _current;
        public static ChangeTrackerDetailsProvider Current
        {
            get
            {
                if (_current == null)
                    _current = new ChangeTrackerDetailsProvider();
                return _current;
            }
            set
            {
                _current = value;
            }
        }

        private Dictionary<string, ChangeTrackerEntityDetailsProvider> entityDetailsProviers = new Dictionary<string, ChangeTrackerEntityDetailsProvider>();
        private DefaultChangeTrackerEntityDetailsProvider defaultDetailsProvider = new DefaultChangeTrackerEntityDetailsProvider();

        public void AddEntityDetailsProvider(string entityName, ChangeTrackerEntityDetailsProvider entityDetailsProvier)
        {
            entityDetailsProviers.Add(entityName, entityDetailsProvier);
        }

        private ChangeTrackerEntityDetailsProvider this[string entityName]
        {
            get
            {
                if (entityDetailsProviers.ContainsKey(entityName))
                    return entityDetailsProviers[entityName];
                else
                    return defaultDetailsProvider;
            }
        }

        public string GetRelatedEntityName(string entityName, string parentEntityName)
        {
            return this[entityName].GetRelationName(entityName, parentEntityName ?? "");
        }

        public RelationInfo GetRelatedEntityOf(string entityName, object entity, DbContext context)
        {
            var provider = this[entityName];
            var result = provider.GetRelatedEntity(entity, context);

            if (result != null && result.Entity == null)//1st: find by key if provided
                result.Entity = provider.GetRelatedEntityByKey(result.EntitySet, result.EntityKey);

            if (result != null && result.EntityKeyName !=null && result.Entity == null)//find by original value if provided : for deleted entities
            {
                var entry = context.Entry(entity);
                if (entry.State == EntityState.Deleted)
                {
                    result.EntityKey = entry.OriginalValues[result.EntityKeyName];
                    result.Entity = provider.GetRelatedEntityByKey(result.EntitySet, result.EntityKey);
                }
            }
            return result;
        }

        public List<T> CorrectToView<T>(List<T> auditDetails, string entityName, DbContext context)
        {
            return this[entityName].CorrectToView(auditDetails, context);
        }

    }


    public class RelationInfo
    {
        public RelationInfo(object entity, object entityKey, string entityKeyName, object entitySet, Func<string> descriptionProvider, Func<bool> registerTrackingForRelatedEntity)
        {
            this.Entity = entity;
            this.EntityKey = entityKey;
            this.EntityKeyName = entityKeyName;
            this.EntitySet = entitySet;
            this.Description = descriptionProvider;
            this.RegisterTrackingForRelatedEntity = registerTrackingForRelatedEntity;
        }

        public object Entity { internal set; get; }
        public Func<string> Description { private set; get; }
        public object EntityKey { internal set; get; }
        public object EntitySet { private set; get; }
        public string EntityKeyName { get; private set; }

        public Func<bool> RegisterTrackingForRelatedEntity { private set; get; }

    }
}
