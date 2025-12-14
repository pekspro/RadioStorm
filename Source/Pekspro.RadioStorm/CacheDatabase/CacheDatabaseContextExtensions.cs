using System.Diagnostics.CodeAnalysis;

namespace Pekspro.RadioStorm.CacheDatabase;

/*
 * This may be able to replace EFCore.BulkExtensions.
public static class CacheDatabaseContextExtensions
{
    public static async Task BulkInsertOrUpdateAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] T>(
        this DbContext context,
        IList<T> entities,
        CancellationToken cancellationToken = default) where T : class
    {
        if (entities == null || entities.Count == 0)
        {
            return;
        }

        foreach (var entity in entities)
        {
            var entry = context.Entry(entity);
            
            if (entry.State == EntityState.Detached)
            {
                var keyValues = context.Model.FindEntityType(typeof(T))!
                    .FindPrimaryKey()!
                    .Properties
                    .Select(p => p.PropertyInfo!.GetValue(entity))
                    .ToArray();

                var existingEntity = await context.Set<T>().FindAsync(keyValues, cancellationToken).ConfigureAwait(false);

                if (existingEntity != null)
                {
                    context.Entry(existingEntity).CurrentValues.SetValues(entity);
                }
                else
                {
                    context.Set<T>().Add(entity);
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public static async Task BulkInsertAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields | DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.Interfaces)] T>(
        this DbContext context,
        IList<T> entities,
        CancellationToken cancellationToken = default) where T : class
    {
        if (entities == null || entities.Count == 0)
        {
            return;
        }

        context.Set<T>().AddRange(entities);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
*/