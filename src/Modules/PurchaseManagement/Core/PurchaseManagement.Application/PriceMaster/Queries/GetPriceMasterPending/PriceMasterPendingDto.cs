namespace PurchaseManagement.Application.PriceMaster.Queries.GetPriceMasterPending;

public sealed class PriceMasterPendingDto
{
    public decimal QuantityFrom { get; set; }
    public decimal QuantityTo { get; set; }
    public decimal UnitPrice { get; set; }
    public int CurrencyId { get; set; }
    public string CurrencyName { get; set; } = string.Empty;
}
