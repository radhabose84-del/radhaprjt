#nullable disable

namespace SalesManagement.Application.Common.Interfaces.IBusinessUnit
{
    public interface IBusinessUnitCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.BusinessUnit businessUnit);
        Task<int> UpdateAsync(Domain.Entities.BusinessUnit businessUnit);
        Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken);
    }
}
