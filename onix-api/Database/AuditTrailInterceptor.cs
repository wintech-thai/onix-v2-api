using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Its.Onix.Api.Models;

public class AuditTrailInterceptor : SaveChangesInterceptor
{
    private readonly RequestContext _requestContext;

    private static readonly HashSet<string> AuditedTables =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "PaymentRequests",
            "PaymentTransactions"
        };

    public AuditTrailInterceptor(RequestContext requestContext)
    {
        _requestContext = requestContext;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        AddAuditTracks(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AddAuditTracks(eventData.Context);

        return base.SavingChangesAsync(
            eventData,
            result,
            cancellationToken);
    }

    private void AddAuditTracks(DbContext? context)
    {
        if (context == null)
            return;

        var entries = context.ChangeTracker
            .Entries()
            .Where(x =>
                x.State == EntityState.Added ||
                x.State == EntityState.Modified ||
                x.State == EntityState.Deleted)
            .Where(x =>
                AuditedTables.Contains(
                    x.Metadata.GetTableName() ?? string.Empty))
            .ToList();

        foreach (var entry in entries)
        {
            var audit = CreateAuditTrack(entry);

            context.Set<MAuditTrack>().Add(audit);
        }
    }

    private MAuditTrack CreateAuditTrack(
        EntityEntry entry)
    {
        var tableName = entry.Metadata.GetTableName()!;

        var primaryKey = entry.Metadata
            .FindPrimaryKey()!
            .Properties
            .First();

        var rowId = entry.Property(primaryKey.Name)
            .CurrentValue?
            .ToString();

        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            // Modified เอาเฉพาะ field ที่เปลี่ยน
            if (entry.State == EntityState.Modified &&
                !property.IsModified)
            {
                continue;
            }

            if (entry.State == EntityState.Modified ||
                entry.State == EntityState.Deleted)
            {
                oldValues[property.Metadata.Name] =
                    property.OriginalValue;
            }

            if (entry.State == EntityState.Added ||
                entry.State == EntityState.Modified)
            {
                newValues[property.Metadata.Name] =
                    property.CurrentValue;
            }
        }

        return new MAuditTrack
        {
            TrackModel = tableName,
            RowId = rowId,
            ActionName = entry.State.ToString(),

            OldValue = oldValues.Count > 0
                ? JsonSerializer.Serialize(oldValues)
                : null,

            NewValue = newValues.Count > 0
                ? JsonSerializer.Serialize(newValues)
                : null,

            IpAddress = _requestContext.IpAddress,
            IpAddress2 = _requestContext.IpAddress2,
            ActionBy = _requestContext.ActionBy,
            OrgId = _requestContext.OrgId,
            ActionRequested = _requestContext.RequestPath,

        };
    }
}
