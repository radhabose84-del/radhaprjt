namespace ProductionManagement.Application.Common.Interfaces.ICountMaster
{
    public interface ICountMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.CountMaster entity);
        Task<int> UpdateAsync(Domain.Entities.CountMaster entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
