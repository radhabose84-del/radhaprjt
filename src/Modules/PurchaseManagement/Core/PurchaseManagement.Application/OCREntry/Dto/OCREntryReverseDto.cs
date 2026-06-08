namespace PurchaseManagement.Application.OCREntry.Dto
{
    /// <summary>
    /// Approval-request payload wrapper for OCR entries. Serialized as { Header, Lines } so that
    /// sp_EvaluateApproval can read header fields via $.Header.* (e.g. $.Header.UnitId) and $.Lines.
    /// OCR has no approval line-items, so Lines is null.
    /// </summary>
    public class OCREntryReverseDto
    {
        public OCREntryWorkFlowDto? Header { get; set; }
        public ICollection<OCREntryWorkFlowDto>? Lines { get; set; }
    }
}
