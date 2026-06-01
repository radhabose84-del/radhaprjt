using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.BlanketMaster;

public class BlanketDetail : BaseEntity, IActivityTracked
{
    public int BlanketHeaderId { get; set; }
    public BlanketHeader? BlanketHeader { get; set; }
    public int ItemSno { get; set; }
    public int ItemId { get; set; }
    public int UOMId { get; set; }
    public decimal EstimatedQuantity { get; set; }
    public decimal Rate { get; set; }
    public decimal TotalPrice { get; set; }
    public int? HSNId { get; set; }
    public decimal? GSTPercentage { get; set; }
    public string? QualitySpecification { get; set; }

    public ICollection<BlanketSchedule> Schedules { get; set; } = new List<BlanketSchedule>();
}
