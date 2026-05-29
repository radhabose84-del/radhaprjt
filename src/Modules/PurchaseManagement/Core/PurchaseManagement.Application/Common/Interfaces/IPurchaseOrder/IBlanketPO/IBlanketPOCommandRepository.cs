using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PurchaseManagement.Application.PurchaseOrder.BlanketPO.Commands.Create;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BlanketPO;
using System.Data.Common;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBlanketPO;

public interface IBlanketPOCommandRepository
{
    Task<int> CreateWithoutTransactionAsync(
        PurchaseOrderHeader poHeader,
        PurchaseBlanketHeader blanketHeader,
        List<PurchaseBlanketDetail> blanketDetails,
        List<PurchasePaymentTerm> paymentTerms,
        CancellationToken ct);

    Task<int> UpdateBlanketPOAsync(
        PurchaseOrderHeader poHeader,
        PurchaseBlanketHeader blanketHeader,
        List<PurchaseBlanketDetail> blanketDetails,
        List<PurchasePaymentTerm> paymentTerms,
        CancellationToken ct);

    Task<bool> CancelAsync(int id, CancellationToken ct);
    Task<bool> ForecloseAsync(int id, CancellationToken ct);
    Task<BlanketPOWorkFlowDto> GetByIdBlanketPOWorkFlowAsync(int id);

    IExecutionStrategy CreateExecutionStrategy();
    Task<(IDbContextTransaction EfTx, DbConnection Conn, DbTransaction DbTx)> BeginTransactionWithConnectionAsync(CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
