using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Greewf.BaseLibrary.MVC.ChangeTracker.ChangeTrackerContext;
using System.Data.Objects;
using System.Data;
using System.Data.Metadata.Edm;

namespace Greewf.BaseLibrary.MVC.ChangeTracker
{
    public abstract class ChangeTracker
    {
        private const string ENTITYKEYPLACEHOLDER = "[NEEDVALUE]";
        private static ChangeTracker current = new DefaultChangeTracker();
        public static ChangeTracker Current
        {
            get
            {
                return current;
            }
            set
            {
                if (value == null)
                    current = new DefaultChangeTracker();
                else
                    current = value;
            }
        }

        public bool AttributesDisabled { get; set; }

        public abstract string UserId { get; }

        public string ConnectionString { get; set; }

        private ChangeTrackerContext.ChangeTrackerContext CreateNewTrackingContext()
        {
            var context = new ChangeTrackerContext.ChangeTrackerContext(ConnectionString);
            context.Configuration.ValidateOnSaveEnabled = false;
            context.Configuration.ProxyCreationEnabled = false;
            context.Configuration.AutoDetectChangesEnabled = false;

            return context;
        }

        public AuditingWidget AuditContext(DbContext context)
        {//TODO : what happened if this method called in different Threads simultanously??

            var trackerContext = CreateNewTrackingContext();
            var widget = new AuditingWidget();
            widget.ChangeTrackerContext = trackerContext;
            widget.TrackedContext = context;
            widget.AuditingDateTime = DateTime.Now;

            var objectContext = ((IObjectContextAdapter)context).ObjectContext;
            var changesList = objectContext.ObjectStateManager.GetObjectStateEntries(~EntityState.Detached);

            //1st: audit entites
            foreach (ObjectStateEntry entry in changesList.Where(o => !o.IsRelationship))
                LogEntry(entry, widget);

            //2nd: audit relations
            foreach (ObjectStateEntry entry in changesList.Where(o => o.IsRelationship))
                LogRelationEntity(entry, widget);

            //3rd : correct audit internal relations
            var trackedEtitiesStateManager = ((IObjectContextAdapter)widget.TrackedContext).ObjectContext.ObjectStateManager;
            var lstDescriptionNeeded = new Dictionary<AuditLog, Func<string>>();
            var lstEnforcedParentTracking = new Dictionary<ObjectStateEntry, Guid>();//the child entities which modified independently but the tracking record should be registered for the related parent (for example when you delete a Contact which is related to a customer, you should have this record in customer tracking log too)

            foreach (var item in widget.TrackedEntities)
            {
                var relatedEntityInfo = ChangeTrackerDetailsProvider.Current.GetRelatedEntityOf(item.Value.EntityName, item.Key.Entity, widget.TrackedContext);
                if (relatedEntityInfo != null && relatedEntityInfo.Entity != null)
                {
                    var relatedEntry = trackedEtitiesStateManager.GetObjectStateEntry(relatedEntityInfo.Entity);

                    if (widget.TrackedEntities.ContainsKey(relatedEntry))//maybe the change tracker didnt track any changes with the parent entity (for example : a contact is changed where the related customer remained unchanged or the contact is changed directly)
                    {
                        item.Value.ParentId = widget.TrackedEntities[relatedEntry].Id;
                        lstDescriptionNeeded.Add(item.Value, relatedEntityInfo.Description);//we do this becuase of performance. lots of items will be removed in pruning process
                    }
                    else if (relatedEntityInfo.RegisterTrackingForRelatedEntity())
                    {
                        item.Value.ParentId = Guid.NewGuid();
                        lstDescriptionNeeded.Add(item.Value, relatedEntityInfo.Description);//we do this becuase of performance. lots of items will be removed in pruning process
                        lstEnforcedParentTracking.Add(relatedEntry, item.Value.ParentId.Value);
                    }

                }
            }

            //4th: track enforced parent 
            foreach (var item in lstEnforcedParentTracking)
            {
                var oldState = item.Key.State;//it should be always unchanged
                objectContext.ObjectStateManager.ChangeObjectState(item.Key.Entity, EntityState.Modified);
                LogEntry(item.Key, widget, item.Value);
                objectContext.ObjectStateManager.ChangeObjectState(item.Key.Entity, oldState);
            }

            //5th : Pruning : remove unnessary audits.
            //5-1st: check audits which are not the parent of others and have no audit details either            
            PruneUnnessaryLeaves(widget, lstDescriptionNeeded);

            //5-2nd : recheck the audits to find if any empty audit remains after previously removed items
            PruneUnnessaryLeaves(widget, lstDescriptionNeeded);

            //6th : complete leaves description
            foreach (var item in lstDescriptionNeeded)
                item.Key.Description = item.Value();

            return widget;
        }

        public void SaveAuditing(AuditingWidget auditingResult)
        {
            var trackedEtitiesStateManager = ((IObjectContextAdapter)auditingResult.TrackedContext).ObjectContext.ObjectStateManager;

            foreach (var item in auditingResult.TrackedEntities)
            {   //it ensure the newly added entities have the Key value (it happeneds for Identity columns)

                if (item.Value.EntityKey == null && item.Value.EventType == "A")//just added items need to regain their ID 
                    item.Value.EntityKey = item.Key.EntityKey.EntityKeyValues == null ? null : item.Key.EntityKey.EntityKeyValues.First().Value.ToString();
            }

            foreach (var item in auditingResult.TrackedEntityDetails)
            {
                if (item.Key.OriginalValue == ENTITYKEYPLACEHOLDER)
                    item.Key.OriginalValue = item.Value.EntityKeyValues.First().Value.ToString();
                else if (item.Key.NewValue == ENTITYKEYPLACEHOLDER)
                    item.Key.NewValue = item.Value.EntityKeyValues.First().Value.ToString();
            }


            //auditingResult.ChangeTrackerContext.ChangeTracker.Entries().ToList().ForEach(o=>o.State = EntityState.Detached) ; //TODO : !!!CHECK!!! It may help you to avoid cotext creating on every tracking
            auditingResult.ChangeTrackerContext.SaveChanges();
            auditingResult.ChangeTrackerContext.Dispose();
        }

        private static void PruneUnnessaryLeaves(AuditingWidget auditingResult, Dictionary<AuditLog, Func<string>> lstNeedDescription)
        {

            var lstRemove = new List<AuditLog>();
            foreach (var al in auditingResult.ChangeTrackerContext.AuditLogs.Local)
            {
                if (al.AuditLogDetails.Count == 0 && auditingResult.ChangeTrackerContext.AuditLogs.Local.Count(o => o.ParentId == al.Id) == 0)
                    lstRemove.Add(al);
            }

            lstRemove.ForEach(o =>
            {
                auditingResult.ChangeTrackerContext.AuditLogs.Remove(o);
                lstNeedDescription.Remove(o);
            });
        }

        private void LogEntry(ObjectStateEntry dbEntry, AuditingWidget widget, Guid? auditId = null)
        {
            // Get primary key value (If you have more than one key column, this will need to be adjusted)
            if (dbEntry.State == System.Data.EntityState.Unchanged || dbEntry.State == System.Data.EntityState.Detached) return;

            var entityType = dbEntry.Entity.GetType();

            //NOTE : proxy creation is done by need. so we should check the name to see if a proxy is created or the main class used.
            if (entityType.Name.LastIndexOf("_") + 1 + 64/*special DbContextId*/ == entityType.Name.Length)
                entityType = entityType.BaseType;//to ignore proxy names

            string entityName = entityType.Name;


            var auditLog = new AuditLog()
            {
                Id = auditId ?? Guid.NewGuid(),
                DateTime = widget.AuditingDateTime,
                UserId = UserId,
                EntityName = entityName.ToLower(),
                EventType = dbEntry.State.ToString()[0].ToString(), // Added       
                EntityKey = dbEntry.EntityKey.EntityKeyValues == null ? null : dbEntry.EntityKey.EntityKeyValues.First().Value.ToString(),//NOTE : Only support tables with a key , TODO : what happened for newly added entities?
            };

            widget.TrackedEntities.Add(dbEntry, auditLog);//we use it to fill "EntityKey" property after saving. (Identity columns get value after SaveChanges() called)

            widget.ChangeTrackerContext.AuditLogs.Add(auditLog);

            LogEnteryDetails(dbEntry, auditLog, widget);

        }

        private void LogRelationEntity(ObjectStateEntry dbEntry, AuditingWidget widget)
        {
            if (dbEntry.State == System.Data.EntityState.Modified)
                throw new Exception("HEY!!!! Modified!!!!!");

            if (dbEntry.State != System.Data.EntityState.Deleted && dbEntry.State != System.Data.EntityState.Added) return;//TODO  : modified relation??? 
            string entityName = "";

            if (dbEntry.EntitySet.BuiltInTypeKind == BuiltInTypeKind.AssociationSet)//n-to-n relation
            {
                var ass = dbEntry.EntitySet as System.Data.Metadata.Edm.AssociationSet;
                entityName = ass.AssociationSetEnds[0].EntitySet.Name;
                var values = dbEntry.State == EntityState.Added ? dbEntry.CurrentValues : dbEntry.OriginalValues;

                var key1 = values.GetValue(0) as EntityKey;
                var key2 = values.GetValue(1) as EntityKey;

                LogRelationEntityEnd(key1, key2, ass.AssociationSetEnds[1], dbEntry.State, widget);
                LogRelationEntityEnd(key2, key1, ass.AssociationSetEnds[0], dbEntry.State, widget);

            }
            else
                return;

        }

        private void LogRelationEntityEnd(EntityKey mainKey, EntityKey relatedKey, AssociationSetEnd relatedAssociationEnd, EntityState relationStatus, AuditingWidget widget)
        {
            var ctx = (widget.TrackedContext as IObjectContextAdapter).ObjectContext;
            var relatedEntry = ctx.ObjectStateManager.GetObjectStateEntry(mainKey);

            if (relatedEntry.State != EntityState.Detached && relatedEntry.State != EntityState.Unchanged)
            {
                var relatedAuditLog = widget.TrackedEntities[relatedEntry];
                LogRelatedEntryDetails(relatedAssociationEnd.EntitySet.Name, relatedKey, relatedAuditLog, widget, relationStatus == EntityState.Added);
            }
        }


        private void LogRelatedEntryDetails(string refrencePropertyName, EntityKey refrenceKey, AuditLog auditLog, AuditingWidget widget, bool isAddedRelation)
        {
            string value = refrenceKey.EntityKeyValues == null ? ENTITYKEYPLACEHOLDER : refrenceKey.EntityKeyValues.First().Value.ToString();

            var auditLogDetail =
                new AuditLogDetail()
                {
                    Id = Guid.NewGuid(),
                    AuditLogId = auditLog.Id,
                    PropertyName = refrencePropertyName,
                    NewValue = isAddedRelation ? value : null,
                    OriginalValue = isAddedRelation ? null : value
                };

            auditLog.AuditLogDetails.Add(auditLogDetail);
            widget.ChangeTrackerContext.AuditLogDetails.Add(auditLogDetail);

            widget.TrackedEntityDetails.Add(auditLogDetail, refrenceKey);
        }


        private void LogEnteryDetails(ObjectStateEntry dbEntry, AuditLog auditLog, AuditingWidget widget)
        {
            if (dbEntry.State == System.Data.EntityState.Added || dbEntry.State == System.Data.EntityState.Deleted)
            {
                bool isAdd = dbEntry.State == EntityState.Added;
                var values = isAdd ? dbEntry.CurrentValues : dbEntry.OriginalValues;

                for (int i = 0; i < values.FieldCount; i++)
                {
                    object objValue = values.GetValue(i);
                    string value = objValue == null ? null : objValue.ToString();

                    if (!string.IsNullOrEmpty(value))
                    {
                        var d = new AuditLogDetail()
                        {
                            Id = Guid.NewGuid(),
                            AuditLogId = auditLog.Id,
                            PropertyName = values.GetName(i),
                            NewValue = isAdd ? value : null,
                            OriginalValue = isAdd ? null : value,
                        };
                        auditLog.AuditLogDetails.Add(d);
                        widget.ChangeTrackerContext.AuditLogDetails.Add(d);
                    }
                }

            }
            else if (dbEntry.State == System.Data.EntityState.Modified)
            {
                for (int i = 0; i < dbEntry.CurrentValues.FieldCount; i++)
                {
                    // For updates, we only want to capture the columns that actually changed
                    var originalValue = dbEntry.OriginalValues.GetValue(i);
                    var currentValue = dbEntry.CurrentValues.GetValue(i);

                    if (!object.Equals(originalValue, currentValue))
                    {
                        var originalString = originalValue == null ? null : originalValue.ToString();
                        var newString = currentValue == null ? null : currentValue.ToString();

                        if (!string.IsNullOrEmpty(originalString) || !string.IsNullOrEmpty(newString))
                        {
                            var d = new AuditLogDetail()
                            {
                                Id = Guid.NewGuid(),
                                AuditLogId = auditLog.Id,
                                PropertyName = dbEntry.OriginalValues.GetName(i),
                                OriginalValue = originalString,
                                NewValue = newString
                            };
                            auditLog.AuditLogDetails.Add(d);
                            widget.ChangeTrackerContext.AuditLogDetails.Add(d);
                        }
                    }
                }
            }

        }

    }


}
