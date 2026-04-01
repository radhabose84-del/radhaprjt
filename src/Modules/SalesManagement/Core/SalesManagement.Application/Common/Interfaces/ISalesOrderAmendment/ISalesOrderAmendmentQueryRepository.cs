using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrderAmendment;

namespace SalesManagement.Application.Common.Interfaces.ISalesOrderAmendment
{
    public interface ISalesOrderAmendmentQueryRepository
    {
        Task<(List<SalesOrderAmendmentHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<SalesOrderAmendmentHeaderDto?> GetByIdAsync(int id);
        Task<(List<PendingSalesOrderAmendmentDto>, int)> GetPendingAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<bool> SalesOrderExistsAndApprovedAsync(int salesOrderHeaderId);
        Task<bool> HasDispatchAdviceAsync(int salesOrderHeaderId);
        Task<bool> HasPendingAmendmentAsync(int salesOrderHeaderId);
        Task<bool> SalesOrderDetailExistsAsync(int salesOrderDetailId, int salesOrderHeaderId);
    }
}
