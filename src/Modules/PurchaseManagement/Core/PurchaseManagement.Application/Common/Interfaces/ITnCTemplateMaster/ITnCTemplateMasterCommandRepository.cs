using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster
{
    public interface ITnCTemplateMasterCommandRepository
    {
        Task<int> CreateAsync(PurchaseManagement.Domain.Entities.TnCTemplateMaster entity, CancellationToken ct);

        Task<bool> UpdateAsync(PurchaseManagement.Domain.Entities.TnCTemplateMaster entity, List<TnCTemplateApplicability>? newApplicabilities);

        Task<bool> SoftDeleteAsync(int id);
    }
}