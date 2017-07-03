using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RevolutionaryStuff.Core.ApplicationParts;

namespace RevolutionaryStuff.Core
{
    public static class EntityFrameworkHelpers
    {
        public static IList<EntityEntry<T>> EntityEntryList<T>(this ChangeTracker changeTracker, params EntityState[] states) where T : class
        {
            return changeTracker.Entries<T>().Where(z => states.Length == 0 ? true : states.Contains(z.State)).ToList();
        }
        public static IList<T> EntityList<T>(this ChangeTracker changeTracker, params EntityState[] states) where T : class
        {
            return changeTracker.Entries<T>().Where(z => states.Length == 0 ? true : states.Contains(z.State)).ConvertAll(z=>z.Entity).ToList();
        }

        public static void PreSaveChanges(this DbContext db)
            => db.PreSaveChanges<int>(null);

        public static void PreSaveChanges<TTenantId>(this DbContext db, Func<TTenantId> tenantIdGetter) where
            TTenantId : IEquatable<TTenantId>
        {
            db.ChangeTracker.EntityList<IPreSave>(EntityState.Added, EntityState.Modified, EntityState.Unchanged).ForEach(z => z.PreSave());

            var noCreates = db.ChangeTracker.EntityList<IDontCreate>(EntityState.Added);
            if (noCreates.Count > 0)
            {
                var typeNames = noCreates.ConvertAll(z => z.GetType().Name).ToSet().ToCsv(false);
                throw new Exception(string.Format("Policy prevents following types from being created using EF: [{0}]", typeNames));
            }

            db.ChangeTracker.EntityList<IValidate>(EntityState.Added, EntityState.Modified).ForEach(z => z.Validate());

            TTenantId tenantId = default(TTenantId);
            bool tenantIdFetched = false;

            db.ChangeTracker.EntityList<ITenanted<TTenantId>>(EntityState.Added).ForEach(
                tid => 
                {
                    var existingTenantId = tid.TenantId;
                    if (default(TTenantId).Equals(existingTenantId))
                    {
                        if (!tenantIdFetched)
                        {
                            tenantId = tenantIdGetter();
                            tenantIdFetched = true;
                        }
                        tid.TenantId = tenantId;
                    }
                } 
            );

            db.ChangeTracker.EntityEntryList<IDeleteOnSave>(EntityState.Modified, EntityState.Unchanged, EntityState.Added).ForEach(
                e=>
                {
                    if (!e.Entity.IsMarkedForDeletion) return;
                    e.State = EntityState.Deleted;
                }
                );
        }
    }
}
