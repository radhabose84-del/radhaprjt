namespace SalesManagement.Application.Common.Interfaces.IStoTypeMaster
{
    public interface IStoTypeMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.StoTypeMaster entity);
        Task<int> UpdateAsync(Domain.Entities.StoTypeMaster entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
