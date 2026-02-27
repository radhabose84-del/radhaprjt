namespace SalesManagement.Application.SalesItemPriceMaster.Dto
{
    public class SalesItemPriceMasterDto
    {
        public int Id { get; set; }
        public string? PriceCode { get; set; }

        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }

        public int SalesSegmentId { get; set; }
        public string? SalesSegmentName { get; set; }

        public int PaymentTermsId { get; set; }
        public string? PaymentTermsCode { get; set; }
        public string? PaymentTermsDescription { get; set; }

        public decimal ExMillPrice { get; set; }

        public int CurrencyId { get; set; }
        public string? CurrencyCode { get; set; }

        public DateOnly ValidFrom { get; set; }
        public DateOnly ValidTo { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
