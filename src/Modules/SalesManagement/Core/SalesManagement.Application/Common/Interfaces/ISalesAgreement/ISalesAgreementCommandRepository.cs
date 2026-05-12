using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.Common.Interfaces.ISalesAgreement
{
    public interface ISalesAgreementCommandRepository
    {
        Task<int> CreateAsync(SalesAgreementHeader entity, int transactionTypeId);
        Task<int> UpdateAsync(SalesAgreementHeader entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
