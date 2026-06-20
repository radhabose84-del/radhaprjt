namespace FinanceManagement.Application.GlAccountMaster.Dto
{
    // One row of the reusable account type-ahead (US-GL02-07). Inactive accounts are returned with
    // IsActive=false so the FE can grey them out (non-selectable); IsFavourite/IsRecent drive ranking.
    public class AccountSearchResultDto
    {
        public int Id { get; set; }
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }
        public int AccountTypeId { get; set; }
        public string? AccountTypeName { get; set; }
        public int AccountGroupId { get; set; }
        public string? AccountGroupName { get; set; }
        public bool IsActive { get; set; }

        // Per-user flags (resolved from the Mongo store in the handler).
        public bool IsFavourite { get; set; }
        public bool IsRecent { get; set; }
    }
}
