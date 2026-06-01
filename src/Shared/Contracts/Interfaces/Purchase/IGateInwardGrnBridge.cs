using Contracts.Dtos.Purchase;

namespace Contracts.Interfaces.Purchase
{
    /// <summary>
    /// Cross-module bridge used by GateEntryManagement to validate and create a GRN row
    /// when a PO-backed Gate Inward is saved. Implementation lives in PurchaseManagement
    /// and wraps the existing <c>CreateGRNEntryCommand</c> via MediatR — the GRN module
    /// itself is untouched.
    /// </summary>
    public interface IGateInwardGrnBridge
    {
        /// <summary>
        /// Validates the would-be GRN command without persisting anything.
        /// Returns an empty list when the input is valid; otherwise the list contains the
        /// validation messages produced by the GRN validator (tolerance breach, partial-receipt
        /// block, etc.). Caller should throw a <c>ValidationException</c> when non-empty so the
        /// Gate Inward save is aborted.
        /// </summary>
        Task<IReadOnlyList<string>> ValidateAsync(
            GateInwardGrnContextDto input, CancellationToken ct = default);

        /// <summary>
        /// Builds the GRN command and dispatches it via MediatR. The MediatR pipeline re-runs
        /// the validator (it should pass — caller already validated) and then the existing
        /// <c>CreateGRNEntryCommandHandler</c> persists the GRN with computed tax totals.
        /// Returns the new GRN row id.
        /// </summary>
        Task<int> CreateAsync(
            GateInwardGrnContextDto input, CancellationToken ct = default);
    }
}
