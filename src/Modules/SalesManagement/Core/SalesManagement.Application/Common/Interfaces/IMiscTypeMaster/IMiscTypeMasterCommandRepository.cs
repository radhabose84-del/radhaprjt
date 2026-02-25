namespace SalesManagement.Application.Common.Interfaces.IMiscTypeMaster
{
    public interface IMiscTypeMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.MiscTypeMaster entity);
        Task<int> UpdateAsync(Domain.Entities.MiscTypeMaster entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
