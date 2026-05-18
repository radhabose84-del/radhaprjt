namespace PurchaseManagement.Application.ContractPO.Dto;

public class ContractPOReleaseHistoryDto
{
    public int Id { get; set; }
    public int ContractPOHeaderId { get; set; }
    public int ContractPODetailId { get; set; }
    public int ReleasePOId { get; set; }
    public string? ReleasePONumber { get; set; }
    public DateTimeOffset ReleaseDate { get; set; }
    public decimal ReleasedQuantity { get; set; }
    public decimal ReleasedRate { get; set; }
    public decimal ReleasedValue { get; set; }
    public string? ItemName { get; set; }
    public string? StatusName { get; set; }
}
