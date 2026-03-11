namespace SalesManagement.Application.Common.Interfaces.IStoReceipt
{
    public interface IStoReceiptCommandRepository
    {
        Task<string> GenerateNextStoReceiptNumberAsync(int receivingPlantId, CancellationToken ct = default);
        Task<int> CreateAsync(Domain.Entities.StoReceiptHeader entity, int packedStatusId);
    }
}
