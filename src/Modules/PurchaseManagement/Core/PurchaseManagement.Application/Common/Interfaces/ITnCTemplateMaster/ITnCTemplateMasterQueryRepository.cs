using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetAllTnCTemplateMaster;
using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetTnCTemplateMasterAutoComplete;

namespace PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster
{
    public interface ITnCTemplateMasterQueryRepository
    {
        Task<(List<TncTemplateMasterDto>, int)> GetAllTncTemplateAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<TncTemplateMasterDto> GetByIdAsync(int id);
        Task<bool> ExistsByTypeAndNameAsync(int templateTypeId, string templateName, int? excludeId = null, CancellationToken ct = default);

        Task<bool> ApplicabilitiesExistAsync(IEnumerable<int> ids, CancellationToken ct = default);
        Task<bool> IsUsedInTransactionsAsync(int templateId, CancellationToken ct = default);

        Task<List<TnCAutoCompleteDto>> GetTnCTemplateAutoCompleteAsync(string? search, int? templateTypeId, int? applicabilityId);
    }
}