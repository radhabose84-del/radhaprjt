using System.Globalization;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FinanceManagement.Infrastructure.Logging
{
    // Writes a property-level change trail to Finance.ActivityLog for every IActivityTracked entity.
    public sealed class ActivityLogSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IIPAddressService _user;
        private readonly ITimeZoneService _timeZone;

        public ActivityLogSaveChangesInterceptor(IIPAddressService user, ITimeZoneService timeZone)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _timeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
        }

        private static readonly HashSet<string> IgnoredProps = new(StringComparer.OrdinalIgnoreCase)
        {
            "Id",
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

            // Snapshot the tracked entries before adding any logs (avoids "collection modified").
            // Insert is intentionally NOT logged — only Update/Delete.
            var tracked = db.ChangeTracker.Entries()
                .Where(e => e.Entity is IActivityTracked &&
                            (e.State == EntityState.Modified ||
                             e.State == EntityState.Deleted))
                .ToList();

            var logs = new List<ActivityLog>(tracked.Count * 2);

            foreach (var entry in tracked)
            {
                var clrType = entry.Metadata.ClrType;
                var (keyName, keyValue, keyAsInt) = GetPrimaryKey(entry);
                var entityId = keyAsInt ?? (entry.Entity as BaseEntity)?.Id ?? 0;

                if (entry.State == EntityState.Deleted)
                {
                    logs.Add(NewLog(clrType.Name, entityId, "Delete", "*", $"Deleted {keyName}={keyValue}", null, now, keyValue));
                    continue;
                }

                // Modified: one row per changed property.
                foreach (var p in entry.Properties.ToList())
                {
                    if (!p.IsModified) continue;

                    var propName = p.Metadata.Name;
                    if (IgnoredProps.Contains(propName)) continue;
                    if (p.Metadata.IsShadowProperty()) continue;
                    if (p.Metadata.IsConcurrencyToken) continue;

                    var oldStr = ToValueString(p.OriginalValue);
                    var newStr = ToValueString(p.CurrentValue);
                    if (string.Equals(oldStr, newStr, StringComparison.Ordinal)) continue;

                    logs.Add(NewLog(clrType.Name, entityId, "Update", propName, oldStr, newStr, now, keyValue));
                }
            }

            if (logs.Count > 0)
                db.ActivityLogs.AddRange(logs);
        }

        private ActivityLog NewLog(string entityName, int entityId, string action, string propertyName,
            string? oldValue, string? newValue, DateTimeOffset now, string keyValue) => new()
        {
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            PropertyName = propertyName,
            OldValue = oldValue,
            NewValue = newValue,
            CreatedDate = now,
            CreatedBy = _user.GetUserId(),
            CreatedByName = _user.GetUserName(),
            CreatedIP = _user.GetSystemIPAddress(),
            Scope = entityName,
            ScopeKey = keyValue
        };

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
                var valObj = entry.State == EntityState.Deleted ? valEntry.OriginalValue : valEntry.CurrentValue;
                var valStr = ToValueString(valObj) ?? "NULL";

                parts.Add($"{name}={valStr}");
                if (keyAsInt is null && valObj is int i) keyAsInt = i;
            }

            return (pk.Properties.Count == 1 ? pk.Properties[0].Name : "CompositeKey",
                    string.Join(",", parts),
                    keyAsInt);
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
}
