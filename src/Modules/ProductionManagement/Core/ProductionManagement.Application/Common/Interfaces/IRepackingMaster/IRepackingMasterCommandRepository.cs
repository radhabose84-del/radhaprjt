namespace ProductionManagement.Application.Common.Interfaces.IRepackingMaster
{
    public interface IRepackingMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.RepackingMaster entity, int typeId);
        Task<int> UpdateAsync(Domain.Entities.RepackingMaster entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
