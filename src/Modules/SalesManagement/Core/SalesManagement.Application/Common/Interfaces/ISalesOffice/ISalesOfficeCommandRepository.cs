
namespace SalesManagement.Application.Common.Interfaces.ISalesOffice
{
    public interface ISalesOfficeCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.SalesOffice entity);
        Task<int> UpdateAsync(Domain.Entities.SalesOffice entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
