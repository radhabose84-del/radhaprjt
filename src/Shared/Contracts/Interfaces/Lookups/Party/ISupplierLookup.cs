using Contracts.Dtos.Lookups.Party;

namespace Contracts.Interfaces.Lookups.Party
{
    /// <summary>
    /// Cross-module lookup for active suppliers (parties of type Supplier) in the ERP Party Master.
    /// Backs the External Service Request vendor selection.
    /// </summary>
    public interface ISupplierLookup
    {
        /// <summary>
        /// Searches active, non-deleted suppliers by Vendor Name or Vendor Code.
        /// </summary>
        Task<IReadOnlyList<SupplierLookupDto>> SearchSuppliersAsync(string? term, CancellationToken ct = default);

        /// <summary>
        /// Returns the supplier if the given party id is an active, non-deleted Supplier; otherwise null.
        /// Used for mandatory-vendor FK validation and server-side VendorName resolution.
        /// </summary>
        Task<SupplierLookupDto?> GetActiveSupplierByIdAsync(int partyId, CancellationToken ct = default);

        /// <summary>
        /// Returns the party if the given id is an active, non-deleted party typed Supplier <b>or</b> Ginner;
        /// otherwise null. Used by OCR, whose "Supplier / Ginner" field sources cotton from either type.
        /// </summary>
        Task<SupplierLookupDto?> GetActiveSupplierOrGinnerByIdAsync(int partyId, CancellationToken ct = default);
    }
}
