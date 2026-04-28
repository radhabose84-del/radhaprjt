using SalesManagement.Application.SalesOrderTypeMaster.Dto;

namespace SalesManagement.Application.Common.Interfaces.ISalesOrderTypeMaster
{
    public interface ISalesOrderTypeMasterQueryRepository
    {
        Task<(List<SalesOrderTypeMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);

        Task<SalesOrderTypeMasterDto?> GetByIdAsync(int id);

        Task<IReadOnlyList<SalesOrderTypeMasterLookupDto>> AutocompleteAsync(string term, CancellationToken cancellationToken);

        // Validation helpers
        Task<bool> NotFoundAsync(int id);

        /// <summary>
        /// Checks whether a (SoTypeId, TaxTypeId) combination already exists among non-deleted rows.
        /// Used by Create/Update validators to enforce composite uniqueness.
        /// </summary>
        Task<bool> AlreadyExistsAsync(int soTypeId, int taxTypeId, int? excludeId = null);

        /// <summary>
        /// Validates that <paramref name="soTypeId"/> references a MiscMaster row whose MiscType
        /// is <c>SOTM_TYPE</c> and is active + not soft-deleted.
        /// </summary>
        Task<bool> IsValidSoTypeAsync(int soTypeId);

        /// <summary>
        /// Returns the <c>MiscMaster.Code</c> of the row referenced by <paramref name="soTypeId"/>,
        /// or <see langword="null"/> if not found. Used for cross-field validation rules
        /// (RateAgr → RequiresValidity; Sample → Min/Max + MaxQty).
        /// </summary>
        Task<string?> GetSoTypeCodeAsync(int soTypeId);

        /// <summary>
        /// Returns the <c>SoTypeId</c> of an existing SalesOrderTypeMaster row by Id,
        /// or <see langword="null"/> if not found / soft-deleted. Used by Update validator
        /// cross-field rules (since SoTypeId is immutable / not in Update payload).
        /// </summary>
        Task<int?> GetSoTypeIdByRowIdAsync(int id);
    }
}
