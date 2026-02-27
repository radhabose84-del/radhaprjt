using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.Common.Interfaces.ISalesQuotation
{
    public interface ISalesQuotationCommandRepository
    {
        Task<int> CreateAsync(SalesQuotationHeader entity);
        Task<int> UpdateAsync(SalesQuotationHeader entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
