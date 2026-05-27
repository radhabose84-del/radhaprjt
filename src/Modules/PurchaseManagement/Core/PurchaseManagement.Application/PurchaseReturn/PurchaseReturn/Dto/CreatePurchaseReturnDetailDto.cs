namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;

public class CreatePurchaseReturnDetailDto
{
    public int GrnDetailId { get; set; }
    public int ItemId { get; set; }
    public int UomId { get; set; }
    public decimal ReceivedQty { get; set; }
    public decimal AcceptedQty { get; set; }
    public decimal ReturnQty { get; set; }
    public decimal? RatePerUnit { get; set; }
    public decimal? LineValue { get; set; }
    public int? ReturnReasonId { get; set; }
    public string? LineRemarks { get; set; }
}
