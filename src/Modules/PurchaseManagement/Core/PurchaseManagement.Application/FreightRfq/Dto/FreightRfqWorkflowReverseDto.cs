namespace PurchaseManagement.Application.FreightRfq.Dto
{
    /// <summary>
    /// Envelope for the Workflow approval-request payload. sp_EvaluateApproval reads
    /// $.Header.UnitId / $.Header.DepartmentId and $.Lines, so the payload must be shaped
    /// as { "Header": {...}, "Lines": [...] } — mirroring CreateBlanketMasterReverseDto.
    /// </summary>
    public sealed class FreightRfqWorkflowReverseDto
    {
        public FreightRfqWorkFlowDto? Header { get; set; }
        public object? Lines { get; set; }
    }
}
