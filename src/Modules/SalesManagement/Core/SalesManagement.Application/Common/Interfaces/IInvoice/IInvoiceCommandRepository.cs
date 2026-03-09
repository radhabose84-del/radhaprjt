using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.Common.Interfaces.IInvoice
{
    public interface IInvoiceCommandRepository
    {
        Task<string> GenerateNextInvoiceNoAsync(int unitId, CancellationToken ct = default);
        Task<int> CreateAsync(InvoiceHeader entity, int unitId, int dispatchedStatusId, int invoicedStatusId);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
