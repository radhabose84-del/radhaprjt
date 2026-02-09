using System.Globalization;
using BudgetManagement.Infrastructure.Data;
using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Domain.Common;
using BudgetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

public sealed class ActivityLogSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IIPAddressService _user;
    private readonly ITimeZoneService _timeZone;

    public ActivityLogSaveChangesInterceptor(IIPAddressService user, ITimeZoneService timeZone)
    {
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _timeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
    }

    // ignore system/noisy props everywhere
    private static readonly HashSet<string> IgnoredProps = new(StringComparer.OrdinalIgnoreCase)
    {
        "Id",
        "Header","Lines",
        "CreatedBy","CreatedByName","CreatedDate","CreatedIP",
        "ModifiedBy","ModifiedByName","ModifiedDate","ModifiedIP",
        "RowVersion","Timestamp"
    };

    public override InterceptionResult<int> SavingChanges(DbContextEventData evt, InterceptionResult<int> result)
    {
        if (evt.Context is ApplicationDbContext db) WriteLogs(db);
        return base.SavingChanges(evt, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData evt, InterceptionResult<int> result, CancellationToken ct = default)
    {
        if (evt.Context is ApplicationDbContext db) WriteLogs(db);
        return base.SavingChangesAsync(evt, result, ct);
    }

    private void WriteLogs(ApplicationDbContext db)
    {
        var tzId = _timeZone.GetSystemTimeZone();
        if (string.IsNullOrWhiteSpace(tzId) || tzId.Equals("India Standard Time", StringComparison.OrdinalIgnoreCase))
            tzId = "Asia/Kolkata";

        TimeZoneInfo tz;
        try { tz = TimeZoneInfo.FindSystemTimeZoneById(tzId); }
        catch { tz = TimeZoneInfo.Local; }

        var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);

        // 1) SNAPSHOT the entries BEFORE adding any logs
        var tracked = db.ChangeTracker.Entries()
            .Where(e => e.Entity is IActivityTracked &&
                        (e.State == EntityState.Added ||
                         e.State == EntityState.Modified ||
                         e.State == EntityState.Deleted))
            .ToList(); // 👈 snapshot to avoid "collection modified" exceptions

        // 2) Collect logs here; add to DbSet AFTER the loop
        var logs = new List<ActivityLog>(tracked.Count * 2);

        foreach (var entry in tracked)
        {
            var clrType   = entry.Metadata.ClrType;
            var entity    = entry.Entity;
            var (keyName, keyValue, keyAsInt) = GetPrimaryKey(entry);
            var entityId  = keyAsInt ?? (entity as BaseEntity)?.Id ?? 0;

            if (entry.State == EntityState.Added)
            {
                logs.Add(new ActivityLog
                {
                    EntityName   = clrType.Name,
                    EntityId     = entityId,
                    Action       = "Insert",
                    PropertyName = "*",
                    OldValue     = null,
                    NewValue     = $"Inserted {keyName}={keyValue}",
                    CreatedDate  = now,
                    CreatedBy    = _user.GetUserId(),
                    CreatedByName= _user.GetUserName(),
                    CreatedIP    = _user.GetSystemIPAddress(),
                    Scope        = clrType.Name,
                    ScopeKey     = keyValue
                });
                continue;
            }

            if (entry.State == EntityState.Deleted)
            {
                logs.Add(new ActivityLog
                {
                    EntityName   = clrType.Name,
                    EntityId     = entityId,
                    Action       = "Delete",
                    PropertyName = "*",
                    OldValue     = $"Deleted {keyName}={keyValue}",
                    NewValue     = null,
                    CreatedDate  = now,
                    CreatedBy    = _user.GetUserId(),
                    CreatedByName= _user.GetUserName(),
                    CreatedIP    = _user.GetSystemIPAddress(),
                    Scope        = clrType.Name,
                    ScopeKey     = keyValue
                });
                continue;
            }

            // Modified: only changed properties
            if (entry.State == EntityState.Modified)
            {
                // Optional micro-optimization: snapshot properties too
                var properties = entry.Properties.ToList();

                foreach (var p in properties)
                {
                    if (!p.IsModified) continue;

                    var propName = p.Metadata.Name;
                    if (IgnoredProps.Contains(propName)) continue;
                    if (p.Metadata.IsShadowProperty()) continue;
                    if (p.Metadata.IsConcurrencyToken) continue;
                    if (IsForeignKey(entry, p)) continue;

                    var original = p.OriginalValue;
                    var current  = p.CurrentValue;

                    // ✅ Skip if logically same (e.g., 45000 vs 45000.00)
                    if (ValuesAreEqual(original, current)) continue;

                    var oldStr = ToValueString(original);
                    var newStr = ToValueString(current);

                    logs.Add(new ActivityLog
                    {
                        EntityName   = clrType.Name,
                        EntityId     = entityId,
                        Action       = "Update",
                        PropertyName = propName,
                        OldValue     = oldStr,
                        NewValue     = newStr,
                        CreatedDate  = now,
                        CreatedBy    = _user.GetUserId(),
                        CreatedByName= _user.GetUserName(),
                        CreatedIP    = _user.GetSystemIPAddress(),
                        Scope        = clrType.Name,
                        ScopeKey     = keyValue
                    });
                }
            }
        }

        // 3) Add logs AFTER the enumeration is complete
        if (logs.Count > 0)
            db.ActivityLogs.AddRange(logs);
    }

    // ---- helpers ----

    private static bool IsForeignKey(EntityEntry entry, PropertyEntry p)
        => entry.Metadata.FindForeignKeys(p.Metadata).Any();

    private static (string KeyName, string KeyValue, int? KeyAsInt) GetPrimaryKey(EntityEntry entry)
    {
        var pk = entry.Metadata.FindPrimaryKey();
        if (pk is null || pk.Properties.Count == 0)
            return ("<no-key>", "N/A", null);

        var parts = new List<string>(pk.Properties.Count);
        int? keyAsInt = null;

        foreach (var prop in pk.Properties)
        {
            var name = prop.Name;
            var valEntry = entry.Property(name);
            var valObj = (entry.State == EntityState.Deleted ? valEntry.OriginalValue : valEntry.CurrentValue);
            var valStr = ToValueString(valObj) ?? "NULL";

            parts.Add($"{name}={valStr}");

            if (keyAsInt is null && valObj is int i) keyAsInt = i;
        }

        return (pk.Properties.Count == 1 ? pk.Properties[0].Name : "CompositeKey",
                string.Join(",", parts),
                keyAsInt);
    }
     private static bool ValuesAreEqual(object? oldVal, object? newVal)
    {
        // both null => equal
        if (oldVal is null && newVal is null) return true;

        // one null, one not => different
        if (oldVal is null || newVal is null) return false;

        // same type & Equal => equal
        if (oldVal.Equals(newVal)) return true;

        // Try numeric comparison if both are convertible
        if (oldVal is IConvertible && newVal is IConvertible)
        {
            try
            {
                var oldDec = Convert.ToDecimal(oldVal, CultureInfo.InvariantCulture);
                var newDec = Convert.ToDecimal(newVal, CultureInfo.InvariantCulture);
                if (oldDec == newDec) return true;
            }
            catch
            {
                // ignore conversion errors, fall through to "not equal"
            }
        }

        

        return false;
    }

    private static string? ToValueString(object? value)
    {
        if (value is null) return null;
        return value switch
        {
            DateOnly d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture),
            DateTimeOffset dto => dto.ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz", CultureInfo.InvariantCulture),
            decimal dec => dec.ToString(CultureInfo.InvariantCulture),
            double dbl => dbl.ToString(CultureInfo.InvariantCulture),
            float flt => flt.ToString(CultureInfo.InvariantCulture),
            bool b => b ? "true" : "false",
            Enum e => e.ToString(),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture)
        };
    }
}
