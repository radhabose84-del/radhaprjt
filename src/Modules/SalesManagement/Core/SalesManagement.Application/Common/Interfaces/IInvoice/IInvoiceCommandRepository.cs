using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.Common.Interfaces.IInvoice
{
    public interface IInvoiceCommandRepository
    {
        Task<int> CreateAsync(InvoiceHeader entity, int unitId, int dispatchedStatusId, int invoicedStatusId, int typeId);
        Task<int> UpdateAsync(InvoiceHeader entity, int unitId, int dispatchedStatusId, int invoicedStatusId);
    }
}
