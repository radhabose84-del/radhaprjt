using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ImportPO;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;

public interface IImportPOCommandRepository
{
    Task<int> CreateAsync(PurchaseOrderHeader aggregate, ImportPOCreateDto dto, CancellationToken ct);
    Task<int> UpdateAsync(PurchaseOrderHeader incoming, ImportPOUpdateDto dto, CancellationToken ct);
    Task<PurchaseOrderHeader?> GetAggregateAsync(int id, CancellationToken ct);
        Task<int> AmendAsync(
        PurchaseOrderHeader existing,
        ImportPOUpdateDto dto,
        PurchaseOrderHeader revisedAuditSeed, // carries audit/lineage defaults
        CancellationToken ct);
    
}
