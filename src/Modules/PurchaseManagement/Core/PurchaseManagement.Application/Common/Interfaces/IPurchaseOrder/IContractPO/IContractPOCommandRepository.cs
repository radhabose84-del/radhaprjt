using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using PurchaseManagement.Domain.Entities.ContractPOMaster;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ContractPO;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;

public interface IContractPOCommandRepository
{
    Task<int> CreateCombinePOAsync(
        PurchaseOrderHeader poHeader,
        PurchaseContractHeader contractHeader,
        List<PurchaseContractDetail> contractDetails,
        List<ContractPOReleaseHistory> releaseHistories,
        int transactionTypeId,
        CancellationToken ct);

    Task<int> UpdateContractPOAsync(
        PurchaseOrderHeader poHeader,
        PurchaseContractHeader contractHeader,
        List<PurchaseContractDetail> contractDetails,
        List<ContractPOReleaseHistory> releaseHistories,
        CancellationToken ct);

    Task<int> DeleteContractPOAsync(int poId, CancellationToken ct);

    Task<int> AmendContractPOAsync(
        int existingPoId,
        PurchaseOrderHeader newPoHeader,
        PurchaseContractHeader newContractHeader,
        List<PurchaseContractDetail> newContractDetails,
        List<ContractPOReleaseHistory> newReleaseHistories,
        int transactionTypeId,
        CancellationToken ct);

    // ── Shared Transaction overloads ─────────────────────────────────────────
    // Use these when the caller manages the transaction (e.g. to enroll
    // outbox events in the same SQL transaction).
    IExecutionStrategy CreateExecutionStrategy();

    /// <summary>
    /// Opens a transaction and returns the EF Core transaction together with
    /// the underlying ADO.NET connection and transaction so the caller can
    /// enroll cross-module Dapper writes in the same SQL transaction.
    /// </summary>
    Task<(IDbContextTransaction EfTx, DbConnection Conn, DbTransaction DbTx)> BeginTransactionWithConnectionAsync(CancellationToken ct);

    Task<int> CreateWithoutTransactionAsync(
        PurchaseOrderHeader poHeader,
        PurchaseContractHeader contractHeader,
        List<PurchaseContractDetail> contractDetails,
        List<ContractPOReleaseHistory> releaseHistories,
        CancellationToken ct);

    Task<int> AmendWithoutTransactionAsync(
        int existingPoId,
        PurchaseOrderHeader newPoHeader,
        PurchaseContractHeader newContractHeader,
        List<PurchaseContractDetail> newContractDetails,
        List<ContractPOReleaseHistory> newReleaseHistories,
        CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
