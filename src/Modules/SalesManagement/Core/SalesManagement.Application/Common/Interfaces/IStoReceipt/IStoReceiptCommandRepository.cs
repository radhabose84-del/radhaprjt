namespace SalesManagement.Application.Common.Interfaces.IStoReceipt
{
    public interface IStoReceiptCommandRepository
    {
        Task<string> GenerateNextStoReceiptNumberAsync(int receivingPlantId, CancellationToken ct = default);
        Task<int> CreateAsync(Domain.Entities.StoReceiptHeader entity, int receivingPlantId, int reservedStatusId, int dispatchedStatusId);
        Task<bool> SoftDeleteAsync(int id, int dispatchedStatusId, int reservedStatusId, CancellationToken ct);
    }
}
