using System.Globalization;
using System.Reflection;
using Contracts.Interfaces;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.ChangeTracking;

public abstract class ItemLogRepositoryBase
{
    protected readonly ApplicationDbContext _db;
    private readonly IIPAddressService _ipAddressService;

    // ✅ Record with two constructors:
    //    - (string?, string?) for callers that already pass strings
    //    - (object?, object?) convenience ctor that stringifies via ToValueString(...)
    public record PropertyChange(string Property, string? OldValue, string? NewValue)
    {
        public PropertyChange(string property, object? oldValue, object? newValue)
            : this(property,
                   ItemLogRepositoryBase.ToValueString(oldValue),
                   ItemLogRepositoryBase.ToValueString(newValue))
        { }
    }

    private static readonly HashSet<string> IgnoredProps = new(StringComparer.OrdinalIgnoreCase)
    {
        "Id", "ItemId",
        "CreatedBy", "CreatedByName", "CreatedDate", "CreatedIP",
        "ModifiedBy", "ModifiedByName", "ModifiedDate", "ModifiedIP",
        "RowVersion", "Timestamp"
    };

    protected ItemLogRepositoryBase(ApplicationDbContext db, IIPAddressService ipAddressService)
    {
        _db = db;
        _ipAddressService = ipAddressService;
    }

    // ---------- value formatting helpers ----------
    private static string? ToValueString(object? value)
    {
        if (value is null) return null;

        return value switch
        {
            DateTime dt        => dt.ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz", CultureInfo.InvariantCulture),
            DateTimeOffset dto => dto.ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz", CultureInfo.InvariantCulture),
            decimal dec        => dec.ToString(CultureInfo.InvariantCulture),
            double d           => d.ToString(CultureInfo.InvariantCulture),
            float f            => f.ToString(CultureInfo.InvariantCulture),
            bool b             => b ? "true" : "false",
            Enum e             => e.ToString(),
            _                  => Convert.ToString(value, CultureInfo.InvariantCulture)
        };
    }

    private static bool IsIgnored(string propName) => IgnoredProps.Contains(propName);

    // ---------- diff helpers ----------
    protected List<PropertyChange> GetModifiedProps(EntityEntry entry)
    {
        var changes = new List<PropertyChange>();

        foreach (var p in entry.Properties)
        {
            if (!p.IsModified) continue;
            if (IsIgnored(p.Metadata.Name)) continue;

            var oldVal = ToValueString(p.OriginalValue);
            var newVal = ToValueString(p.CurrentValue);

            if (!string.Equals(oldVal, newVal, StringComparison.Ordinal))
                changes.Add(new PropertyChange(p.Metadata.Name, oldVal, newVal));
        }

        return changes;
    }

    protected List<PropertyChange> DiffByReflection<T>(T original, T updated)
    {
        var changes = new List<PropertyChange>();
        if (original is null || updated is null) return changes;

        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var pi in props)
        {
            if (!pi.CanRead) continue;
            if (IsIgnored(pi.Name)) continue;

            var ov = ToValueString(pi.GetValue(original));
            var nv = ToValueString(pi.GetValue(updated));

            if (!string.Equals(ov, nv, StringComparison.Ordinal))
                changes.Add(new PropertyChange(pi.Name, ov, nv));
        }

        return changes;
    }

    // ---------- logging helpers ----------
    protected bool TryAddUpdateLog(string entityName, int entityId, IEnumerable<PropertyChange> changes)
    {
        if (changes is null) return false;

        var material = changes
            .Where(c => !string.Equals(c.OldValue, c.NewValue, StringComparison.Ordinal))
            .ToList();

        if (material.Count == 0) return false;

        foreach (var c in material)
        {
            _db.ItemLog.Add(new InventoryManagement.Domain.Entities.Item.ItemDetail.ItemLog
            {
                EntityName   = entityName,
                EntityId     = entityId,
                Action       = "Update",
                PropertyName = c.Property,
                OldValue     = c.OldValue,
                NewValue     = c.NewValue,
                CreatedBy    = _ipAddressService.GetUserId(),
                CreatedDate  = DateTime.UtcNow,
                CreatedByName= _ipAddressService.GetUserName(),
                CreatedIP    = _ipAddressService.GetSystemIPAddress()
            });
        }

        return true;
    }
}
