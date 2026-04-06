namespace SalesManagement.Application.Common.Interfaces.IFreightMaster
{
    public interface IFreightMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.FreightMaster entity);
        Task<int> UpdateAsync(Domain.Entities.FreightMaster entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
