namespace PurchaseManagement.Application.PurchaseOrder.Dtos.BlanketPO;

public class BlanketPOPaymentTermItem
{
    public int Id { get; set; }
    public int PaymentTermId { get; set; }
    public int? PaymentModeId { get; set; }
    public decimal? PaymentPercentage { get; set; }
    public decimal? PaymentAmount { get; set; }
    public decimal? AdvancePercent { get; set; }
    public int? CreditDays { get; set; }
    public int? PaymentModelId { get; set; }
    public int? InsuranceId { get; set; }
    public int? InsurancePercent { get; set; }
    public decimal? InsuranceAmount { get; set; }
    public decimal? AdvanceAmount { get; set; }
    public decimal? BalancePercent { get; set; }
    public decimal? BalanceAmount { get; set; }
    public string? Remarks { get; set; }
}
