using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Create;
using PurchaseManagement.Domain.Entities.PurchaseOrder;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;

public interface IImportPOCommandRepository
{
    Task<ImportPOWorkFlowDto> GetByIdImportPOWorkFlowAsync(int id);

    // ── Original methods (self-contained transactions) ───────────────────────
    Task<int> CreateAsync(PurchaseOrderHeader aggregate, ImportPOCreateDto dto, CancellationToken ct);
    Task<int> UpdateAsync(PurchaseOrderHeader incoming, ImportPOUpdateDto dto, CancellationToken ct);
    Task<PurchaseOrderHeader?> GetAggregateAsync(int id, CancellationToken ct);
    Task<int> AmendAsync(
        PurchaseOrderHeader existing,
        ImportPOUpdateDto dto,
        PurchaseOrderHeader revisedAuditSeed,
        CancellationToken ct);

    // ── Shared Transaction overloads ─────────────────────────────────────────
    // Use these when the caller manages the transaction (e.g. to enroll a
    // cross-module Dapper write in the same SQL transaction).
    IExecutionStrategy CreateExecutionStrategy();
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct);

    /// <summary>
    /// Opens a transaction and returns the EF Core transaction together with
    /// the underlying ADO.NET connection and transaction so the caller can
    /// enroll cross-module Dapper writes in the same SQL transaction.
    /// </summary>
    Task<(IDbContextTransaction EfTx, DbConnection Conn, DbTransaction DbTx)> BeginTransactionWithConnectionAsync(CancellationToken ct);

    Task<int> CreateWithoutTransactionAsync(PurchaseOrderHeader aggregate, ImportPOCreateDto dto, CancellationToken ct);
    Task<int> UpdateWithoutTransactionAsync(PurchaseOrderHeader incoming, ImportPOUpdateDto dto, CancellationToken ct);
    Task<int> AmendWithoutTransactionAsync(
        PurchaseOrderHeader existing,
        ImportPOUpdateDto dto,
        PurchaseOrderHeader revisedAuditSeed,
        CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);

    // ── Cancel / Foreclose ───────────────────────────────────────────────────
    Task<bool> CancelAsync(int id, CancellationToken ct);
    Task<bool> ForecloseAsync(int id, CancellationToken ct);
}
