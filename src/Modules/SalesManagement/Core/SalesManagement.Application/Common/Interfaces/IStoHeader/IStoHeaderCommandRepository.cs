namespace SalesManagement.Application.Common.Interfaces.IStoHeader
{
    public interface IStoHeaderCommandRepository
    {
        Task<string> GenerateNextStoNumberAsync(int supplyingPlantId, CancellationToken ct = default);
        Task<int> CreateAsync(Domain.Entities.StoHeader entity);
        Task<int> UpdateAsync(Domain.Entities.StoHeader entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
