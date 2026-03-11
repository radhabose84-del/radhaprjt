namespace SalesManagement.Application.Common.Interfaces.IDeliveryChallan
{
    public interface IDeliveryChallanCommandRepository
    {
        Task<string> GenerateNextDeliveryNumberAsync(int fromPlantId, CancellationToken ct = default);
        Task<int> CreateAsync(Domain.Entities.DeliveryChallanHeader entity, int fromPlantId, int packedStatusId, int dispatchedStatusId);
        Task<bool> SoftDeleteAsync(int id, int dispatchedStatusId, int packedStatusId, CancellationToken ct);
    }
}
