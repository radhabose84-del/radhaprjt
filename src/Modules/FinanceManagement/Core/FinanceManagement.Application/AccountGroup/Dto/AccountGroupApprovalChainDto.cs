namespace FinanceManagement.Application.AccountGroup.Dto
{
    // One level of the Account Group Move approval chain — read-only, for the modal banner
    // ("Approvals: 1) Finance Controller → 2) CFO"). Sourced from config; the workflow engine
    // enforces the actual routing.
    public class AccountGroupApprovalChainDto
    {
        public int Level { get; set; }
        public string? Label { get; set; }
    }
}
