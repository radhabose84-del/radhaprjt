using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.Common.Interfaces.ISalesOrder
{
    public interface ISalesOrderCommandRepository
    {
        Task<string> GenerateNextSalesOrderNoAsync(int unitId, CancellationToken ct = default);
        Task<int> CreateAsync(SalesOrderHeader entity);
        Task<int> UpdateAsync(SalesOrderHeader entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
        Task<bool> UpdateVisitNotesAttachmentAsync(int id, string fileName, CancellationToken ct);
        Task<bool> UpdateAgentPOAttachmentAsync(int id, string fileName, CancellationToken ct);
    }
}
