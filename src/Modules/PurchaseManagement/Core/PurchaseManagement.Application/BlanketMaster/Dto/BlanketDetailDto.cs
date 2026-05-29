namespace PurchaseManagement.Application.BlanketMaster.Dto;

public class BlanketDetailDto
{
    public int Id { get; set; }
    public int BlanketHeaderId { get; set; }
    public int ItemSno { get; set; }
    public int ItemId { get; set; }
    public string? ItemName { get; set; }
    public int UOMId { get; set; }
    public string? UOMName { get; set; }
    public decimal EstimatedQuantity { get; set; }
    public decimal Rate { get; set; }
    public decimal TotalPrice { get; set; }
    public int? HSNId { get; set; }
    public string? HSNCode { get; set; }
    public decimal? GSTPercentage { get; set; }
    public string? QualitySpecification { get; set; }

    public List<BlanketScheduleDto> Schedules { get; set; } = new();
}
