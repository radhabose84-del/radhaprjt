namespace SalesManagement.Application.AgentCommissionConfig.Dto
{
    public class AgentCommissionConfigDto
    {
        public int Id { get; set; }

        public int AgentId { get; set; }
        public string? AgentName { get; set; }

        public int SalesSegmentId { get; set; }
        public string? SegmentName { get; set; }

        public int CommissionTypeId { get; set; }
        public string? CommissionTypeName { get; set; }

        public int? CommissionBasisId { get; set; }
        public string? CommissionBasisName { get; set; }

        public int? ApplicableLevelId { get; set; }
        public string? ApplicableLevelName { get; set; }

        public decimal CommissionPercentage { get; set; }

        public int? CurrencyId { get; set; }
        public string? CurrencyCode { get; set; }

        public DateTimeOffset ValidityFrom { get; set; }
        public DateTimeOffset ValidityTo { get; set; }

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
