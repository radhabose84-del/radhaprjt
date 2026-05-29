namespace PurchaseManagement.Application.BlanketMaster.Dto;

public class BlanketScheduleDto
{
    public int Id { get; set; }
    public int BlanketDetailId { get; set; }
    public int ScheduleNo { get; set; }
    public DateTimeOffset ScheduleDate { get; set; }
    public decimal ScheduleQuantity { get; set; }
    public string? Remarks { get; set; }
}
