namespace FinanceManagement.Application.CoaChangeRequest.Dto
{
    // US-GL02-08B — a dual-approval unfreeze request (status + both approval slots + window).
    public class CoaUnfreezeRequestDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string? Reason { get; set; }
        public int? CfoApproverUserId { get; set; }
        public DateTimeOffset? CfoApprovedOn { get; set; }
        public int? SysAdminApproverUserId { get; set; }
        public DateTimeOffset? SysAdminApprovedOn { get; set; }
        public string? Status { get; set; }
        public int WindowMinutes { get; set; }
        public DateTimeOffset? WindowOpenedOn { get; set; }
        public DateTimeOffset? WindowExpiry { get; set; }
        public int RequestedByUserId { get; set; }
        public DateTimeOffset? RequestedOn { get; set; }
    }
}
