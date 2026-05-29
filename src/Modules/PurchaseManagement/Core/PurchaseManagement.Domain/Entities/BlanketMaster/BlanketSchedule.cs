using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.BlanketMaster;

public class BlanketSchedule : BaseEntity, IActivityTracked
{
    public int BlanketDetailId { get; set; }
    public BlanketDetail? BlanketDetail { get; set; }
    public int ScheduleNo { get; set; }
    public DateTimeOffset ScheduleDate { get; set; }
    public decimal ScheduleQuantity { get; set; }
    public string? Remarks { get; set; }
}
