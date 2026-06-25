using FinanceManagement.Application.CoaReport.Dto;

namespace FinanceManagement.Application.Common.Interfaces.ICoaReport
{
    // US-GL02-15 COA Listing & Structure Reports — read-only Dapper aggregates over the COA.
    public interface ICoaReportQueryRepository
    {
        // AC1/AC5 — full COA listing (hierarchy + attributes + posting count + FS-mapping), ordered by code.
        Task<List<CoaListingItemDto>> GetCoaListingAsync(
            int companyId, int? accountTypeId, int? accountGroupId, bool activeOnly, string? searchTerm, CancellationToken ct);

        // AC2/AC3 — accounts never posted or with no posting in the last @monthsSincePosting months.
        Task<List<AccountUsageItemDto>> GetAccountUsageAsync(int companyId, int monthsSincePosting, CancellationToken ct);

        // AC4 — Schedule III FS-mapping validation: count of leaf groups + the unmapped ones.
        Task<FsMappingValidationDto> GetFsMappingValidationAsync(int companyId, CancellationToken ct);
    }
}
