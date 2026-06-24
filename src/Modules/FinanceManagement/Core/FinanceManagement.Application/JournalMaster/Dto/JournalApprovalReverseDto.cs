namespace FinanceManagement.Application.JournalMaster.Dto
{
    // Approval-workflow payload wrapper. sp_EvaluateApproval reads $.Header.UnitId.
    public sealed class JournalApprovalReverseDto
    {
        public JournalHeaderDto? Header { get; set; }
        public List<JournalDetailDto>? Lines { get; set; }
    }
}
