using PurchaseManagement.Domain.Entities.PurchaseOrder.ImportPO;

namespace PurchaseManagement.Domain.Entities;

public sealed class ExchangeRate
{
    public int Id { get; set; }
    public string BaseCurrency { get; set; } = default!;   // INR
    public string QuoteCurrency { get; set; } = default!;  // USD, EUR
    public decimal Rate { get; set; }                      // 1 base = rate quote
    public decimal? ActualRate { get; set; }
    public DateOnly EffectiveDate { get; set; }            // API "date"
    public string Source { get; set; } = "Frankfurter";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedOnUtc { get; set; }
    public ICollection<ImportPOHeader>? importPOHeaderExRate{ get; set; }
    
}
