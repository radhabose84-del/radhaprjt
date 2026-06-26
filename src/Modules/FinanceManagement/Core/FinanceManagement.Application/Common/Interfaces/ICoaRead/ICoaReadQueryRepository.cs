using FinanceManagement.Application.CoaRead.Dto;

namespace FinanceManagement.Application.Common.Interfaces.ICoaRead
{
    // US-GL02-16 COA Read API — fast, downstream-facing reads over GlAccountMaster.
    public interface ICoaReadQueryRepository
    {
        // AC1 — single account by code (unique index CompanyId+AccountCode); < 100ms. Null if not found.
        Task<CoaAccountReadDto?> GetByCodeAsync(int companyId, string accountCode, CancellationToken ct);

        // AC5 — accounts filtered by type/group, each returned WITH its active status.
        Task<List<CoaAccountReadDto>> SearchByTypeGroupAsync(
            int companyId, int? accountTypeId, int? accountGroupId, bool activeOnly, CancellationToken ct);

        // AC2 — fields needed to validate posting (active + currency + CC). Null if the code doesn't exist.
        Task<AccountPostingInfo?> GetPostingInfoByCodeAsync(int companyId, string accountCode, CancellationToken ct);

        // AC3 — minimal info by id for the deactivation hook (prior state + event payload).
        Task<AccountPostingInfo?> GetPostingInfoByIdAsync(int companyId, int id, CancellationToken ct);
    }
}
