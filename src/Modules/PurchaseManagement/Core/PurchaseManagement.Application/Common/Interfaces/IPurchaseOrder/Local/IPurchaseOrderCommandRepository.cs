
using Microsoft.EntityFrameworkCore.Storage;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using PurchaseManagement.Domain.Entities.PurchaseOrder;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;

public interface IPurchaseOrderCommandRepository
{
        Task<int> CreateWithoutTransactionAsync(PurchaseOrderHeader aggregate, CancellationToken ct);
        Task<int> UpdateAsync(PurchaseOrderHeader incoming, PurchaseOrderUpdateDto dto, CancellationToken ct);
        Task<int> SoftDeleteAsync(int id, CancellationToken ct);
        Task<string> GenerateNextCodeAsync(int POCategoryId, int? poMethodId, DateTimeOffset poDate, string unitCode, CancellationToken ct = default);
        Task<bool> UpdatePOApproveAsync(int id, int statusId, CancellationToken ct = default);
        Task<int> AmendAsync(PurchaseOrderHeader existing,  PurchaseOrderUpdateDto dto, CancellationToken ct);
        Task<PurchaseOrderHeader?> GetAggregateAsync(int id, CancellationToken ct);
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct);
        Task SaveChangesAsync(CancellationToken ct);
        IExecutionStrategy CreateExecutionStrategy();
        Task RecomputeIndentPoQtyAsync(HashSet<int> indentHeaderIds, CancellationToken ct);
}


