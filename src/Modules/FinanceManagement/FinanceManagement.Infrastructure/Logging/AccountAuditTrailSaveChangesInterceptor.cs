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
    // US-GL02-09 — writes the immutable, field-level statutory change trail to Finance.AccountAuditTrail
    // for every IAuditTrailed entity (the COA structural masters + governance entities).
    //
    // Update / Delete are captured in SavingChanges and persisted in the SAME transaction as the change
    // (AC-1). Insert is captured in SavingChanges (the DB-generated Id is not yet known) and finalised in
    // SavedChanges once the key exists, via a single follow-up save — the canonical EF Core audit pattern.
    //
    // Registered scoped (one instance per DbContext per scope), so the pending-insert buffer is safe.
    public sealed class AccountAuditTrailSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IIPAddressService _user;
        private readonly ITimeZoneService _timeZone;

        // Added IAuditTrailed entries captured pre-save; finalised in SavedChanges when their Id is known.
        private readonly List<PendingInsert> _pendingInserts = new();

        public AccountAuditTrailSaveChangesInterceptor(IIPAddressService user, ITimeZoneService timeZone)
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

        // ---- SavingChanges: capture Update/Delete in-transaction, buffer Inserts ----

        public override InterceptionResult<int> SavingChanges(DbContextEventData evt, InterceptionResult<int> result)
        {
            if (evt.Context is ApplicationDbContext db) Capture(db);
            return base.SavingChanges(evt, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData evt, InterceptionResult<int> result, CancellationToken ct = default)
        {
            if (evt.Context is ApplicationDbContext db) Capture(db);
            return base.SavingChangesAsync(evt, result, ct);
        }

        // ---- SavedChanges: flush buffered Inserts now that Ids are populated ----

        public override int SavedChanges(SaveChangesCompletedEventData evt, int result)
        {
            if (evt.Context is ApplicationDbContext db) FlushInserts(db);
            return base.SavedChanges(evt, result);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData evt, int result, CancellationToken ct = default)
        {
            if (evt.Context is ApplicationDbContext db) await FlushInsertsAsync(db, ct);
            return await base.SavedChangesAsync(evt, result, ct);
        }

        private void Capture(ApplicationDbContext db)
        {
            var now = Now();

            var tracked = db.ChangeTracker.Entries()
                .Where(e => e.Entity is IAuditTrailed &&
                            (e.State == EntityState.Added ||
                             e.State == EntityState.Modified ||
                             e.State == EntityState.Deleted))
                .ToList();

            var logs = new List<AccountAuditTrail>(tracked.Count * 2);

            foreach (var entry in tracked)
            {
                var entityName = entry.Metadata.ClrType.Name;
                var (keyName, keyValue, keyAsInt) = GetPrimaryKey(entry);
                var companyId = GetCompanyId(entry);

                if (entry.State == EntityState.Added)
                {
                    // Id is not yet assigned — buffer the changed values and finalise in SavedChanges.
                    var props = entry.Properties
                        .Where(p => p.Metadata.Name != "Id" && !IgnoredProps.Contains(p.Metadata.Name)
                                    && !p.Metadata.IsShadowProperty() && !p.Metadata.IsConcurrencyToken)
                        .Select(p => (Name: p.Metadata.Name, Value: ToValueString(p.CurrentValue)))
                        .ToList();
                    _pendingInserts.Add(new PendingInsert(entry, entityName, companyId, now, props));
                    continue;
                }

                var entityId = keyAsInt ?? (entry.Entity as BaseEntity)?.Id ?? 0;

                if (entry.State == EntityState.Deleted)
                {
                    logs.Add(NewLog(entityName, entityId, companyId, "Delete", "*",
                        $"Deleted {keyName}={keyValue}", null, now, keyValue));
                    continue;
                }

                // Modified — one row per changed property.
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

                    logs.Add(NewLog(entityName, entityId, companyId, "Update", propName, oldStr, newStr, now, keyValue));
                }
            }

            if (logs.Count > 0)
                db.AccountAuditTrails.AddRange(logs);
        }

        private List<AccountAuditTrail> DrainPendingInserts()
        {
            if (_pendingInserts.Count == 0) return new List<AccountAuditTrail>(0);

            var pending = _pendingInserts.ToList();
            _pendingInserts.Clear();

            var logs = new List<AccountAuditTrail>(pending.Count * 2);
            foreach (var p in pending)
            {
                var entityId = (p.Entry.Entity as BaseEntity)?.Id ?? 0;
                var keyValue = $"Id={entityId}";
                foreach (var (name, value) in p.Properties)
                    logs.Add(NewLog(p.EntityName, entityId, p.CompanyId, "Insert", name, null, value, p.When, keyValue));
            }
            return logs;
        }

        private void FlushInserts(ApplicationDbContext db)
        {
            var logs = DrainPendingInserts();
            if (logs.Count == 0) return;
            db.AccountAuditTrails.AddRange(logs);
            db.SaveChanges();   // re-enters the interceptor harmlessly: no IAuditTrailed Added, buffer is empty
        }

        private async Task FlushInsertsAsync(ApplicationDbContext db, CancellationToken ct)
        {
            var logs = DrainPendingInserts();
            if (logs.Count == 0) return;
            await db.AccountAuditTrails.AddRangeAsync(logs, ct);
            await db.SaveChangesAsync(ct);
        }

        private DateTimeOffset Now()
        {
            var tzId = _timeZone.GetSystemTimeZone();
            if (string.IsNullOrWhiteSpace(tzId) || tzId.Equals("India Standard Time", StringComparison.OrdinalIgnoreCase))
                tzId = "Asia/Kolkata";

            TimeZoneInfo tz;
            try { tz = TimeZoneInfo.FindSystemTimeZoneById(tzId); }
            catch { tz = TimeZoneInfo.Local; }

            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);
        }

        private AccountAuditTrail NewLog(string entityName, int entityId, int companyId, string action,
            string propertyName, string? oldValue, string? newValue, DateTimeOffset now, string keyValue) => new()
        {
            EntityName = entityName,
            EntityId = entityId,
            CompanyId = companyId,
            Action = action,
            PropertyName = propertyName,
            OldValue = oldValue,
            NewValue = newValue,
            CreatedDate = now,
            CreatedBy = _user.GetUserId(),
            CreatedByName = _user.GetUserName(),
            CreatedByRole = _user.GetUserRole(),
            CreatedIP = _user.GetSystemIPAddress(),
            Scope = entityName,
            ScopeKey = keyValue
        };

        private static int GetCompanyId(EntityEntry entry)
        {
            var prop = entry.Metadata.FindProperty("CompanyId");
            if (prop != null)
            {
                var val = entry.Property("CompanyId").CurrentValue;
                if (val is int i) return i;
            }
            return 0;
        }

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

        private sealed record PendingInsert(
            EntityEntry Entry,
            string EntityName,
            int CompanyId,
            DateTimeOffset When,
            List<(string Name, string? Value)> Properties);
    }
}
