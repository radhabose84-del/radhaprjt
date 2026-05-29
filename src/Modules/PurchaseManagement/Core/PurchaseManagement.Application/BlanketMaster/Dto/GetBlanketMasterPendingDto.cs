namespace PurchaseManagement.Application.BlanketMaster.Dto;

public sealed class GetBlanketMasterPendingDto
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string? ItemName { get; set; }
    public int UOMId { get; set; }
    public string? UOMName { get; set; }
    public decimal EstimatedQuantity { get; set; }
    public decimal Rate { get; set; }
    public decimal TotalPrice { get; set; }
    public int? HSNId { get; set; }
    public decimal? GSTPercentage { get; set; }
}
