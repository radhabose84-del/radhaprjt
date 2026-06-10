namespace PurchaseManagement.Application.FreightRfq.Dto
{
    /// <summary>
    /// Minimal Freight RFQ projection serialized as the approval-request payload for the Workflow module.
    /// UnitId is taken from the token at submit time. Mirrors OCREntryWorkFlowDto.
    /// </summary>
    public sealed class FreightRfqWorkFlowDto
    {
        public int Id { get; set; }
        public string? FreightRfqNumber { get; set; }
        public int? SupplierId { get; set; }
        public int StatusId { get; set; }
        public int UnitId { get; set; }
    }
}
