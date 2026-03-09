namespace SalesManagement.Application.Common.Interfaces.IDeliveryChallan
{
    public interface IDeliveryChallanCommandRepository
    {
        Task<string> GenerateNextDeliveryNumberAsync(int fromPlantId, CancellationToken ct = default);
        Task<int> CreateAsync(Domain.Entities.DeliveryChallanHeader entity, int fromPlantId, int packedStatusId, int reservedStatusId);
        Task<bool> SoftDeleteAsync(int id, int reservedStatusId, int packedStatusId, CancellationToken ct);
    }
}
