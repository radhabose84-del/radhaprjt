#nullable disable
namespace SalesManagement.Application.Common.Interfaces.ISalesOrganisation
{
    public interface ISalesOrganisationCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.SalesOrganisation entity);
        Task<int> UpdateAsync(Domain.Entities.SalesOrganisation entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
