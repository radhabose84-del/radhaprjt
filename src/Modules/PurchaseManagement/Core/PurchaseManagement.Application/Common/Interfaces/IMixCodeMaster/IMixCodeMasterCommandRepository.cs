namespace PurchaseManagement.Application.Common.Interfaces.IMixCodeMaster
{
    public interface IMixCodeMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.MixCodeMaster entity);
        Task<int> UpdateAsync(Domain.Entities.MixCodeMaster entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
