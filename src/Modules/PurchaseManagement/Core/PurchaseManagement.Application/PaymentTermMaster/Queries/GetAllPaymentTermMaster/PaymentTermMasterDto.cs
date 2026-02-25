namespace PurchaseManagement.Application.PaymentTermMaster.Queries.GetAllPaymentTermMaster
{
    public class PaymentTermMasterDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Description { get; set; } = default!;
        public int BaselineTypeId { get; set; }
        public int CreditDays { get; set; }
        public decimal? AdvancePercent { get; set; }
        public decimal BalancePercent { get; private set; }
        public decimal? DiscountPercent { get; set; }
        public int? DiscountDays { get; set; }
        public int? GraceDays { get; set; }
        public bool ApplicableForPortal { get; set; }  
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string CreatedByName { get; set; } = default!;   
        public string CreatedIP { get; set; } = default!;
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }  

          public List<PaymentTermInstallmentDto>? Installments { get; set; }   

    }
}