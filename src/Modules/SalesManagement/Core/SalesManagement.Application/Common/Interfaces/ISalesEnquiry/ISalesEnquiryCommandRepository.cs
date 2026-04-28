using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.Common.Interfaces.ISalesEnquiry
{
    public interface ISalesEnquiryCommandRepository
    {
        Task<int> CreateAsync(SalesEnquiryHeader entity, int transactionTypeId);
        Task<int> UpdateAsync(SalesEnquiryHeader entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
