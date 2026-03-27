namespace ProductionManagement.Application.Common.Interfaces.IQualityMaster
{
    public interface IQualityMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.QualityMaster entity);
        Task<int> UpdateAsync(Domain.Entities.QualityMaster entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
