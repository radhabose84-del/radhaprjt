using Contracts.Dtos.Lookups.Party;

namespace Contracts.Interfaces.Lookups.Party
{
    public interface IBankAccountLookup
    {
        /// <summary>
        /// Returns true if the bank account exists (active, not deleted) AND is tagged with the given owner type
        /// (MiscMaster.Code, e.g. "Unit"). Used by owner masters to validate an assigned BankAccountId.
        /// </summary>
        Task<bool> ExistsForOwnerTypeAsync(int bankAccountId, string ownerTypeCode, CancellationToken ct = default);

        /// <summary>Single bank account by Id (for populating the owner master DTO).</summary>
        Task<BankAccountLookupDto?> GetByIdAsync(int bankAccountId, CancellationToken ct = default);

        /// <summary>Active, non-deleted bank accounts of the given owner type (MiscMaster.Code) — for owner dropdowns.</summary>
        Task<IReadOnlyList<BankAccountLookupDto>> GetByOwnerTypeAsync(string ownerTypeCode, string? term, CancellationToken ct = default);
    }
}
