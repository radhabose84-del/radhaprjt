namespace PurchaseManagement.Application.PriceMaster.Queries.GetPriceMasterPending;

public sealed class PriceMasterPendingRowDto
{
    // group (header) columns
    public int      Id           { get; set; }
    public int      ItemId       { get; set; }
    public int      VendorId     { get; set; }
    public DateOnly ValidFrom    { get; set; }
    public DateOnly? ValidTo     { get; set; }
    public int      UomId        { get; set; }
    public string?  CreatedBy    { get; set; }
    public DateTimeOffset CreatedDate { get; set; }

    // line (detail) columns
    public decimal  ScaleQtyFrom { get; set; }
    public decimal? ScaleQtyTo   { get; set; }
    public decimal  UnitPrice    { get; set; }
    public int      CurrencyId   { get; set; }
}
