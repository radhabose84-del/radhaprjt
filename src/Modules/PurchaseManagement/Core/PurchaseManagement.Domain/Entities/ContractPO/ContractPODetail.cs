using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.ContractPO;

public class ContractPODetail : BaseEntity, IActivityTracked
{
    public int ContractPOHeaderId { get; set; }
    public ContractPOHeader? ContractPOHeader { get; set; }
    public int ItemSno { get; set; }
    public int ItemId { get; set; }
    public int UOMId { get; set; }
    public decimal ContractQuantity { get; set; }
    public decimal ContractRate { get; set; }
    public decimal ContractValue { get; set; }
    public decimal UtilizedQuantity { get; set; }
    public decimal BalanceQuantity { get; set; }
    public decimal UtilizedValue { get; set; }
    public decimal BalanceValue { get; set; }
    public int? HSNId { get; set; }
    public decimal? GSTPercentage { get; set; }

    public ICollection<ContractPOReleaseHistory> ContractPOReleaseHistories { get; set; } = new List<ContractPOReleaseHistory>();
}
