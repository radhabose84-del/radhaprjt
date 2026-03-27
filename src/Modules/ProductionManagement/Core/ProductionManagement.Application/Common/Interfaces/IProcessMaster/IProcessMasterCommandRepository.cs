namespace ProductionManagement.Application.Common.Interfaces.IProcessMaster
{
    public interface IProcessMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.ProcessMaster entity);
        Task<int> UpdateAsync(Domain.Entities.ProcessMaster entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
