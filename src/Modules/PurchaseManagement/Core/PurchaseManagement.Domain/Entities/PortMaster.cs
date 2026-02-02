
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ImportPO;

namespace PurchaseManagement.Domain.Entities;

public sealed class PortMaster : BaseEntity
{
    public string PortCode { get; set; } = default!;
    public string PortName { get; set; } = default!;
    public int CountryId { get; set; }
    public int? TypeId { get; set; }
    public MiscMaster MiscType { get; set; } = default!;
    public int? PortTypeId { get; set; }
    public MiscMaster MiscPortType { get; set; } = default!;

    public ICollection<ImportPOHeader>? importPOHeaderShipPort { get; set; }
    public ICollection<ImportPOHeader>? importPOHeaderDestPort { get; set; }
}
