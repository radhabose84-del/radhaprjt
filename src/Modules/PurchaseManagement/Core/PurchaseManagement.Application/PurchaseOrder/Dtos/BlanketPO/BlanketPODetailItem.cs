namespace PurchaseManagement.Application.PurchaseOrder.Dtos.BlanketPO;

public class BlanketPODetailItem
{
    public int Id { get; set; }
    public int BlanketDetailId { get; set; }
    public int ItemSno { get; set; }
    public int ItemId { get; set; }
    public string? ItemName { get; set; }
    public int UOMId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal ItemValue { get; set; }
    public int? DiscountTypeId { get; set; }
    public decimal? DiscountValue { get; set; }
    public int? PandFType { get; set; }
    public decimal? PandFCharge { get; set; }
    public decimal? OtherCharge { get; set; }
    public decimal? GSTPercentage { get; set; }
    public decimal? CGSTPercentage { get; set; }
    public decimal? SGSTPercentage { get; set; }
    public decimal? IGSTPercentage { get; set; }
    public decimal? CGST { get; set; }
    public decimal? SGST { get; set; }
    public decimal? IGST { get; set; }
    public DateTimeOffset? ScheduleDate { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
}
