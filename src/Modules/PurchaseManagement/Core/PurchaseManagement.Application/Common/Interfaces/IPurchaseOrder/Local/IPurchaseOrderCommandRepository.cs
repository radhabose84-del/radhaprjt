using Microsoft.EntityFrameworkCore.Storage;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using PurchaseManagement.Domain.Entities.PurchaseOrder;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;

public interface IPurchaseOrderCommandRepository
{
        Task<int> CreateAsync(PurchaseOrderHeader aggregate, CancellationToken ct);

        /// <summary>
        /// Creates a purchase order without managing transaction internally.
        /// Caller is responsible for transaction management (Begin/Commit/Rollback).
        /// Use this when you need to include other operations in the same transaction.
        /// </summary>
        Task<int> CreateWithoutTransactionAsync(PurchaseOrderHeader aggregate, CancellationToken ct);

        Task<int> UpdateAsync(PurchaseOrderHeader incoming, PurchaseOrderUpdateDto dto, CancellationToken ct);
        Task<int> SoftDeleteAsync(int id, CancellationToken ct);
        Task<string> GenerateNextCodeAsync(int POCategoryId, int? poMethodId, DateTimeOffset poDate, string unitCode, CancellationToken ct = default);
        Task<bool> UpdatePOApproveAsync(int id, int statusId, CancellationToken ct = default);
        Task<int> AmendAsync(PurchaseOrderHeader existing,  PurchaseOrderUpdateDto dto, CancellationToken ct);
        Task<PurchaseOrderHeader?> GetAggregateAsync(int id, CancellationToken ct);

        /// <summary>
        /// Begins a database transaction. Caller must Commit or Rollback.
        /// </summary>
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct);

        /// <summary>
        /// Saves all pending changes to the database (within current transaction if any).
        /// </summary>
        Task SaveChangesAsync(CancellationToken ct);

        /// <summary>
        /// Provides the execution strategy for retriable transactions.
        /// </summary>
        IExecutionStrategy CreateExecutionStrategy();

        /// <summary>
        /// Recomputes POQty for impacted indents after PO operations.
        /// </summary>
        Task RecomputeIndentPoQtyAsync(HashSet<int> indentHeaderIds, CancellationToken ct);
}


