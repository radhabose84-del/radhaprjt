namespace PurchaseManagement.Application.OCREntry.Dto
{
    /// <summary>
    /// Minimal OCR projection serialized as the approval-request payload for the Workflow module.
    /// UnitId is taken from the token at create time (OCREntry itself does not persist a UnitId).
    /// </summary>
    public sealed class OCREntryWorkFlowDto
    {
        public int Id { get; set; }
        public string? OcrNumber { get; set; }
        public int SupplierId { get; set; }
        public int StatusId { get; set; }
        public int UnitId { get; set; }
    }
}
