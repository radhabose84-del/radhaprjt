namespace Contracts.Dtos.Workflow
{
    /// <summary>
    /// Final-approval summary for a workflow transaction — the name of the approver who
    /// last approved it and when. Null fields mean the transaction is not yet approved.
    /// </summary>
    public sealed class ApprovalInfoDto
    {
        public string? ApproverName { get; set; }
        public DateTimeOffset? ApprovedDate { get; set; }
    }
}
