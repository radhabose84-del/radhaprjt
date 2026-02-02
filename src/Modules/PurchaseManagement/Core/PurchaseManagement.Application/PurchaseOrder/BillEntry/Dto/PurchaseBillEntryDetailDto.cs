namespace PurchaseManagement.Application.PurchaseOrder.BillEntry.Dto;

public sealed class PurchaseBillEntryDetailDto
{
    public int? Id { get; set; }             
    public int ItemId { get; set; }    
    public int? GrnDetailId { get; set; }
    public int PoDetailId { get; set; }
    public decimal PoQty { get; set; }
    public decimal GrnQty { get; set; }
    public decimal BilledQty { get; set; }
    public decimal PoRate { get; set; }
    public decimal BilledRate { get; set; }
    public int? UomId { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal DiscountAmount { get; set; }

    // Calculated values – for GETs
    public decimal LineBaseAmount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal CgstAmount { get; set; }
    public decimal SgstAmount { get; set; }
    public decimal IgstAmount { get; set; }
    public decimal LineTotal { get; set; }
}
