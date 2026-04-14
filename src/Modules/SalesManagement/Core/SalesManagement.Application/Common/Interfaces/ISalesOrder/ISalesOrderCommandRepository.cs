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
        Task<bool> FinalizeOrderStatusAsync(SalesOrderHeader entity);
    }
}
