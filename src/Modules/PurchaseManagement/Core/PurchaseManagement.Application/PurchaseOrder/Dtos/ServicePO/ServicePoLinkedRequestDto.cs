namespace PurchaseManagement.Application.PurchaseOrder.Dtos.ServicePO
{
    /// <summary>
    /// Result of grouping a Service PO's lines by their linked MaintenanceRequest.
    /// Consumed by ApprovedRejectedConsumer to bump each ESR's ConvertedToPoAmount
    /// by the sum of PlannedValue across all lines that reference it.
    /// </summary>
    public sealed class ServicePoLinkedRequestDto
    {
        public int RequestId { get; set; }
        public decimal TotalPlannedValue { get; set; }
    }
}
