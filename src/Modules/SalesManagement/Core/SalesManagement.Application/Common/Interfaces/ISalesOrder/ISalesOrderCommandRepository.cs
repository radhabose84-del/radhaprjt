using SalesManagement.Application.SalesOrder.Commands.CreateSalesOrder;
using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.Common.Interfaces.ISalesOrder
{
    public interface ISalesOrderCommandRepository
    {
        Task<int> CreateAsync(SalesOrderHeader entity, int transactionTypeId);
        Task<int> UpdateAsync(SalesOrderHeader entity);
        Task<bool> CancelAsync(int id, CancellationToken ct);
        Task<bool> ForecloseAsync(int id, CancellationToken ct);
        Task<bool> UpdateVisitNotesAttachmentAsync(int id, string fileName, CancellationToken ct);
        Task<bool> UpdateAgentPOAttachmentAsync(int id, string fileName, CancellationToken ct);
        Task<bool> UpdateMdApprovalDocumentAsync(int id, string fileName, CancellationToken ct);
        Task<SalesOrderWorkFlowDto> GetByIdSalesOrderWorkFlowAsync(int id);
        Task<SalesOrderHeader?> GetByIdEntityAsync(int id);
        Task<bool> FinalizeOrderStatusAsync(SalesOrderHeader entity, int modifiedBy, string? modifiedByName, string? modifiedIP);

        // Read-only same-module fetch used by the handler to decide whether to skip workflow/notifications
        // for "Rate Agreement"-type orders (compared by TypeName, not by Id).
        Task<SalesManagement.Domain.Entities.SalesOrderTypeMaster?> GetSalesOrderTypeMasterByIdAsync(int id);
    }
}
