using FinanceManagement.Application.GlAccountMaster.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IGlAccountMaster
{
    public interface IGlAccountMasterQueryRepository
    {
        Task<(List<GlAccountMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int companyId, int? accountTypeId = null, int? accountGroupId = null);
        Task<GlAccountMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<GlAccountMasterLookupDto>> AutocompleteAsync(string term, int companyId, string? accountTypeCode, CancellationToken ct);

        Task<bool> AlreadyExistsByCodeAsync(string accountCode, int companyId, int? id = null);
        Task<bool> AlreadyExistsByNameAsync(string accountName, int companyId, int? id = null);
        Task<bool> NotFoundAsync(int id);

        Task<bool> AccountTypeExistsForCompanyAsync(int accountTypeId, int companyId);
        Task<bool> AccountGroupExistsForCompanyAsync(int accountGroupId, int companyId);
        Task<bool> NormalBalanceExistsAsync(int normalBalanceId);
        Task<bool> SubLedgerTypeExistsAsync(int subLedgerTypeId);

        Task<(int AccountCodeLength, string? StartCode, string? AccountTypeName)?> GetAccountTypeFormatAsync(int accountTypeId);

        Task<bool> SoftDeleteValidationAsync(int id);
        Task<bool> IsGlAccountLinkedAsync(int id);
    }
}
