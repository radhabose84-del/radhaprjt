namespace Contracts.Interfaces.Operations.MaintenanceManagement
{
    /// <summary>
    /// Cross-module write contract used by PurchaseManagement's ApprovedRejectedConsumer
    /// to flip a MaintenanceRequest's status when a Service PO that references it is approved.
    /// Not named '*Lookup' on purpose — must NOT be wrapped by the lookup cache decorator
    /// (writes can't be cached).
    /// </summary>
    public interface IMaintenanceRequestConversionService
    {
        /// <summary>
        /// Adds <paramref name="valueDelta"/> to the request's ConvertedToPoAmount
        /// (clamped to 0) and flips its status to one of:
        /// Open (when amount &lt;= 0), FullyConverted (amount &gt;= EstimatedServiceCost),
        /// otherwise PartiallyConverted.
        /// Returns true if a row was updated.
        /// </summary>
        Task<bool> ApplyServicePoConversionAsync(
            int requestId, decimal valueDelta, CancellationToken ct = default);
    }
}
