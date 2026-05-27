namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Dto;

public class ReturnableQtyDto
{
    public int GrnDetailId { get; set; }
    public int ItemId { get; set; }
    public string? ItemCode { get; set; }
    public string? ItemName { get; set; }
    public int UomId { get; set; }
    public string? UomName { get; set; }
    public decimal ReceivedQty { get; set; }
    public decimal AcceptedQty { get; set; }
    public decimal PriorReturnedQty { get; set; }
    public decimal ReturnableQty { get; set; }
    public decimal? RatePerUnit { get; set; }
}
