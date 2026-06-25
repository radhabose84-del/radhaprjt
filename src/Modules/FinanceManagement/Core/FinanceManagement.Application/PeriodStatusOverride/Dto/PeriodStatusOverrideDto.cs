namespace FinanceManagement.Application.PeriodStatusOverride.Dto
{
    public class PeriodStatusOverrideDto
    {
        public int Id { get; set; }
        public int FinancialPeriodId { get; set; }
        public string? FinancialYearCode { get; set; }
        public string? PeriodName { get; set; }
        public int CompanyId { get; set; }

        public int FromStatusId { get; set; }
        public string? FromStatusCode { get; set; }
        public string? FromStatusName { get; set; }

        public int ToStatusId { get; set; }
        public string? ToStatusCode { get; set; }
        public string? ToStatusName { get; set; }

        public int RequestedBy { get; set; }
        public DateTimeOffset RequestedAt { get; set; }
        public string? RequestedReason { get; set; }

        public int? CfoApproverId { get; set; }
        public DateTimeOffset? CfoApprovedAt { get; set; }

        public int? SysAdminApproverId { get; set; }
        public DateTimeOffset? SysAdminApprovedAt { get; set; }

        public int OverrideStatusId { get; set; }
        public string? OverrideStatusCode { get; set; }     // 'PENDING' / 'FULLYAPPROVED' / 'APPLIED' / 'REJECTED'
        public string? OverrideStatusName { get; set; }

        public DateTimeOffset? AppliedAt { get; set; }
        public string? RejectionReason { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
    }
}
