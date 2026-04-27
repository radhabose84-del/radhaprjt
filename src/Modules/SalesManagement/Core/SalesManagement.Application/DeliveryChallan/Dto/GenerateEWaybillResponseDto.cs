namespace SalesManagement.Application.DeliveryChallan.Dto
{
    /// <summary>
    /// Shape returned from POST /api/deliveryChallan/{id}/generate-ewaybill.
    /// AlreadyExisted signals to the frontend whether we created a new draft or returned
    /// the one already attached to the DC.
    /// </summary>
    public sealed class GenerateEWaybillResponseDto
    {
        public int EWaybillHeaderId { get; set; }
        public string? DeliveryNumber { get; set; }
        public string? EwbStatus { get; set; }
        public string? EwbNumber { get; set; }
        public bool AlreadyExisted { get; set; }

        // Populated only when validation fails (Option 1 — no DB row written).
        // Caller surfaces each entry to the operator so they can fix master data and retry.
        public List<string>? Errors { get; set; }
    }
}
