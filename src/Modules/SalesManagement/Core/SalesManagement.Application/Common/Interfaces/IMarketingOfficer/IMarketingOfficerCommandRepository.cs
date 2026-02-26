namespace SalesManagement.Application.Common.Interfaces.IMarketingOfficer
{
    public interface IMarketingOfficerCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.MarketingOfficer entity);
        Task<int> UpdateAsync(Domain.Entities.MarketingOfficer entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
