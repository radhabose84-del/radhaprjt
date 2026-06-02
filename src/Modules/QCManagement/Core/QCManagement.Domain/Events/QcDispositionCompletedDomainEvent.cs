using MediatR;

namespace QCManagement.Domain.Events
{
    /// <summary>
    /// Raised after a QC disposition is saved. Downstream modules (PurchaseManagement /
    /// InventoryManagement) subscribe to create putaway rules / stock-ledger movement.
    /// This ticket only fires the event (see SCRUM-1667 scope).
    /// </summary>
    public sealed record QcDispositionCompletedDomainEvent(
        int QcInspectionHdrId,
        string QcInspectionNo,
        int GrnHeaderId,
        int GrnDetailId,
        int ItemId,
        int QcStatusId,
        string QcStatusCode,
        decimal AcceptedQuantity,
        decimal RejectedQuantity,
        int ReceivedUomId,
        string? DispositionRemarks) : INotification;
}
