namespace SalesManagement.Application.Common.Interfaces.IDeliveryChallan
{
    public interface IDeliveryChallanCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.DeliveryChallanHeader entity, int fromPlantId, int packedStatusId, int dispatchedStatusId, int typeId);
        Task<bool> SoftDeleteAsync(int id, int dispatchedStatusId, int packedStatusId, CancellationToken ct);
    }
}
