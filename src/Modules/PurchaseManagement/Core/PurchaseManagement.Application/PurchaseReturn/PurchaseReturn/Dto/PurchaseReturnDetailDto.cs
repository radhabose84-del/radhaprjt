namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;

public class PurchaseReturnDetailDto
{
    public int Id { get; set; }
    public int PurchaseReturnHeaderId { get; set; }
    public int GrnDetailId { get; set; }
    public int ItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public int UomId { get; set; }
    public string? UomName { get; set; }
    public decimal ReceivedQty { get; set; }
    public decimal AcceptedQty { get; set; }
    public decimal ReturnQty { get; set; }
    public decimal? RatePerUnit { get; set; }
    public decimal? LineValue { get; set; }
    public int? ReturnReasonId { get; set; }
    public string? ReturnReasonName { get; set; }
    public string? LineRemarks { get; set; }
    public bool IsActive { get; set; }
}
