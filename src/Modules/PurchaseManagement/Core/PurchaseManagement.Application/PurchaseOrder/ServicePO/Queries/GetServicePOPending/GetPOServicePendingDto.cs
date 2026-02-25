namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.GetServicePOPending
{

    public sealed class GetPOServicePendingDto
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public int ServicePoHeaderId { get; set; }
    public int LineNumber { get; set; }
    public int RequestId { get; set; }
    public int ServiceId { get; set; }
    public string ServiceDescription { get; set; } = "";
    public int UOMId { get; set; }
    public decimal PlannedQuantity { get; set; }
    public decimal PlannedRate { get; set; }
    public string? DiscountType { get; set; }
    public decimal? Discount { get; set; }
    public decimal ItemCost { get; set; }
    public decimal? OtherCost { get; set; }
    public decimal? OtherCharges { get; set; }
    public decimal GstPercent { get; set; }
    public string? Remarks { get; set; }

   
    public int Edit { get; set; }
    public string? EditReason { get; set; }

    }
}