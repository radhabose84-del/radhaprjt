namespace SalesManagement.Application.SalesAgreement.Dto
{
    public class SalesAgreementHeaderDto
    {
        public int Id { get; set; }
        public string? AgreementNo { get; set; }

        public int StatusId { get; set; }
        public string? StatusName { get; set; }

        public DateOnly ValidFrom { get; set; }
        public DateOnly ValidTo { get; set; }

        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }

        public int SalesGroupId { get; set; }
        public string? SalesGroupName { get; set; }

        public int PaymentTermsId { get; set; }
        public string? PaymentTermsName { get; set; }

        public string? Remarks { get; set; }
        public string? CustomerPoRefno { get; set; }
        public string? AgentPOAttachment { get; set; }

        // Capturing Unit (set on create from JWT/IP context; not in command DTO).
        public int? UnitId { get; set; }
        public string? UnitName { get; set; }

        // Rollup fields — populated by GetAllAsync (aggregated from SalesAgreementDetail).
        // Always 0 on GetById (line-level values are available in SalesAgreementDetails directly).
        public decimal TotalQty { get; set; }
        public decimal TotalReleasedQty { get; set; }
        public decimal BalanceQty => TotalQty - TotalReleasedQty;

        // Derived from IsActive + ValidTo:
        //   IsActive=1 AND ValidTo >= today → "Active"
        //   IsActive=1 AND ValidTo <  today → "Expired"
        //   IsActive=0                       → "Inactive"
        public string? Status { get; set; }

        // "Y" when the agreement is eligible for cancel/release (Active + Approved + BalanceQty > 0),
        // "N" otherwise (Expired, Inactive, non-Approved status, or no balance left).
        public string? CancelFlag { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }

        public List<SalesAgreementDetailDto>? SalesAgreementDetails { get; set; }
    }
}
