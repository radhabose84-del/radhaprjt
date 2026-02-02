using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.PurchaseOrder;

public class PurchasePaymentTerm : BaseEntity, IActivityTracked
{
    public int PurchaseOrderId { get; set; }
    public PurchaseOrderHeader? PurchaseTerm { get; set; }
    public int PaymentTermId { get; set; }
    public MiscMaster? MiscPOPaymentTerm { get; set; }
    public decimal? AdvancePercent { get; set; }
    public int? CreditDays { get; set; }
    public int? PaymentModelId { get; set; }
    public MiscMaster? MiscPOPaymentMode{ get; set; }
    public int? InsuranceId { get; set; }
    public int? InsurancePercent { get; set; }
    public decimal? InsuranceAmount { get; set; }
    public decimal? AdvanceAmount { get; set; }
    public decimal? BalancePercent { get; set; }
    public decimal? BalanceAmount { get; set; }    
}
