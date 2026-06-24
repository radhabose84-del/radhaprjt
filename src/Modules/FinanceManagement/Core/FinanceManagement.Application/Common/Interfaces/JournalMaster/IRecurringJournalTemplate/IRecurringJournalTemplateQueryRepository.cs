using FinanceManagement.Application.JournalMaster.Dto;

namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate
{
    public interface IRecurringJournalTemplateQueryRepository
    {
        Task<(List<RecurringJournalTemplateHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<RecurringJournalTemplateHeaderDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<RecurringJournalTemplateLookupDto>> AutocompleteAsync(string term, CancellationToken ct);

        Task<bool> AlreadyExistsByNameAsync(string templateName, int? id = null);
        Task<bool> NotFoundAsync(int id);

        // FK validation helpers (same-module direct SQL).
        Task<bool> VoucherTypeExistsAsync(int voucherTypeId, int companyId);
        Task<bool> FrequencyExistsAsync(int miscId);
        Task<bool> AmountAdjustmentRuleExistsAsync(int miscId);
        Task<bool> GlAccountExistsAsync(int glAccountId, int companyId);
        Task<bool> CostCentreExistsAsync(int costCentreId);
        Task<bool> ProfitCentreExistsAsync(int profitCentreId);
    }
}
