namespace SalesManagement.Application.Common.Interfaces.IStoReceipt
{
    public interface IStoReceiptCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.StoReceiptHeader entity, int packedStatusId, int damagedStatusId, int dispatchedStatusId, int typeId);
    }
}
