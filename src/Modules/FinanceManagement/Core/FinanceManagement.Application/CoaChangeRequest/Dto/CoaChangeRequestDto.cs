namespace FinanceManagement.Application.CoaChangeRequest.Dto
{
    // US-GL02-08B — a change request in the freeze-change workflow (list / detail).
    public class CoaChangeRequestDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int? TargetAccountId { get; set; }
        public int? TargetAccountGroupId { get; set; }
        public string? AccountCodeSnapshot { get; set; }
        public string? ChangeType { get; set; }
        public string? Justification { get; set; }
        public string? ImpactAssessment { get; set; }
        public int? ImpactApprovedByUserId { get; set; }
        public DateTimeOffset? ImpactApprovedOn { get; set; }
        public int? UnfreezeRequestId { get; set; }
        public string? Status { get; set; }
        public bool IsPostFreeze { get; set; }
        public int? CommittedByUserId { get; set; }
        public DateTimeOffset? CommittedOn { get; set; }
        public int RequestedByUserId { get; set; }
        public DateTimeOffset? RequestedOn { get; set; }
    }
}
