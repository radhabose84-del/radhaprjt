namespace SalesManagement.Application.SalesItemPriceMaster.Dto
{
    public class SalesItemPriceMasterDto
    {
        public int Id { get; set; }
        public string PriceCode { get; set; } = null!;

        public int ItemId { get; set; }
        public string ItemCode { get; set; } = null!;
        public string ItemName { get; set; } = null!;

        public int SalesSegmentId { get; set; }
        public string SalesSegmentName { get; set; } = null!;

        public int PaymentTermsId { get; set; }
        public string PaymentTermsCode { get; set; } = null!;
        public string PaymentTermsDescription { get; set; } = null!;

        public decimal ExMillPrice { get; set; }

        public int CurrencyId { get; set; }
        public string? CurrencyCode { get; set; }

        public DateTimeOffset ValidFrom { get; set; }
        public DateTimeOffset ValidTo { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string CreatedByName { get; set; } = null!;
        public string CreatedIP { get; set; } = null!;
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string ModifiedByName { get; set; } = null!;
        public string ModifiedIP { get; set; } = null!;
    }
}
