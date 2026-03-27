namespace ProductionManagement.Application.Common.Interfaces.ICertificationMaster
{
    public interface ICertificationMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.CertificationMaster entity);
        Task<int> UpdateAsync(Domain.Entities.CertificationMaster entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
