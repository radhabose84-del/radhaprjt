namespace SalesManagement.Application.Common.Interfaces.IDeliveryChallan
{
    public interface IDeliveryChallanCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.DeliveryChallanHeader entity, int packedStatusId, int reservedStatusId, int typeId);
        Task<bool> SoftDeleteAsync(int id, int reservedStatusId, int packedStatusId, CancellationToken ct);
    }
}
