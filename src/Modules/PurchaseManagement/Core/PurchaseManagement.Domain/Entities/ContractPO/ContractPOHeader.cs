using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.ContractPO;

public class ContractPOHeader : BaseEntity, IActivityTracked
{
    public int UnitId { get; set; }
    public string ContractPONumber { get; set; } = default!;
    public DateTimeOffset ContractDate { get; set; }
    public int VendorId { get; set; }
    public int CurrencyId { get; set; }
    public DateTimeOffset ValidityFrom { get; set; }
    public DateTimeOffset ValidityTo { get; set; }
    public decimal TotalContractValue { get; set; }
    public decimal UtilizedValue { get; set; }
    public decimal BalanceValue { get; set; }
    public int StatusId { get; set; }
    public MiscMaster? MiscStatus { get; set; }
    public string? Remarks { get; set; }

    public ICollection<ContractPODetail> ContractPODetails { get; set; } = new List<ContractPODetail>();
    public ICollection<ContractPOReleaseHistory> ContractPOReleaseHistories { get; set; } = new List<ContractPOReleaseHistory>();
}
