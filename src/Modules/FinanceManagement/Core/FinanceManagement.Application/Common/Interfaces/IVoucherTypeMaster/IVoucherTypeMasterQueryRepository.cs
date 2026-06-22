using FinanceManagement.Application.VoucherType.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster
{
    public interface IVoucherTypeMasterQueryRepository
    {
        Task<(List<VoucherTypeMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId = null, int? financialYearId = null);
        Task<VoucherTypeMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<VoucherTypeLookupDto>> AutocompleteAsync(string term, int? companyId, CancellationToken ct);
        Task<VoucherTypeSummaryDto> GetSummaryAsync(int? companyId);
        Task<List<VoucherTypeNumberSeriesDto>> GetNumberSeriesAsync(int financialYearId, int? companyId);

        Task<bool> AlreadyExistsByCodeAsync(string voucherTypeCode, int companyId, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> IsSystemAsync(int id);
        Task<bool> AccountTypeExistsAsync(int accountTypeId, int companyId);

        // Delete guard (Rule #25): blocked when the type has consumed a number (LastUsedNumber > 0).
        Task<bool> SoftDeleteValidationAsync(int id);
    }
}
