namespace SalesManagement.Application.Common.Interfaces.IStoHeader
{
    public interface IStoHeaderCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.StoHeader entity, int typeId);
        Task<int> UpdateAsync(Domain.Entities.StoHeader entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
