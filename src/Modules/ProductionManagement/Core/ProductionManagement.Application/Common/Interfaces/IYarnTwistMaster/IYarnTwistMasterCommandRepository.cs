namespace ProductionManagement.Application.Common.Interfaces.IYarnTwistMaster
{
    public interface IYarnTwistMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.YarnTwistMaster entity);
        Task<int> UpdateAsync(Domain.Entities.YarnTwistMaster entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
