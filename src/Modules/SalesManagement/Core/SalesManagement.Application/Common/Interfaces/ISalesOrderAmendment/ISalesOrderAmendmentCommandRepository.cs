using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.Common.Interfaces.ISalesOrderAmendment
{
    public interface ISalesOrderAmendmentCommandRepository
    {
        Task<int> CreateAsync(SalesOrderAmendmentHeader entity, List<SalesOrderAmendmentDetail> details);
        Task<bool> ApplyAmendmentAsync(int amendmentHeaderId, string status, CancellationToken ct);
        Task<SalesOrderAmendmentHeader?> GetByIdEntityAsync(int id);
        Task<SalesOrderHeader?> GetSalesOrderEntityAsync(int salesOrderHeaderId);
    }
}
