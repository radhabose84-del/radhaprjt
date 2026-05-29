using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.BlanketMaster;

public class BlanketHeader : BaseEntity, IActivityTracked
{
    public int UnitId { get; set; }
    public string BlanketNumber { get; set; } = default!;
    public DateTimeOffset BlanketDate { get; set; }
    public int VendorId { get; set; }
    public int CurrencyId { get; set; }
    public int ProcurementTypeId { get; set; }
    public MiscMaster? MiscProcurementType { get; set; }
    public string? BrokerName { get; set; }
    public DateTimeOffset ValidityFrom { get; set; }
    public DateTimeOffset ValidityTo { get; set; }
    public int StatusId { get; set; }
    public MiscMaster? MiscStatus { get; set; }
    public decimal TotalEstimatedValue { get; set; }
    public string? Remarks { get; set; }

    public ICollection<BlanketDetail> Details { get; set; } = new List<BlanketDetail>();
}
