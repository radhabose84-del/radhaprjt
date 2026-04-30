using SalesManagement.Application.Invoice.Commands.CreateInvoice;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.Common.Interfaces.IInvoice
{
    public interface IInvoiceCommandRepository
    {
        Task<int> CreateAsync(InvoiceHeader entity, int unitId, int dispatchedStatusId, int invoicedStatusId, int typeId);
        Task<int> UpdateAsync(InvoiceHeader entity, int unitId, int dispatchedStatusId, int invoicedStatusId);
        Task UpdateApprovalStatusAsync(int id, string status, int modifiedBy, string? modifiedByName, string? modifiedIP, CancellationToken ct);
        Task UpdateInvoiceStatusIdAsync(int id, int statusId, CancellationToken ct);
        Task<InvoiceWorkFlowDto> GetByIdInvoiceWorkFlowAsync(int id);
    }
}
