namespace SalesManagement.Application.Common.Interfaces.ICustomerVisit
{
    public interface ICustomerVisitCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.CustomerVisit entity);
        Task<int> UpdateAsync(Domain.Entities.CustomerVisit entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
        Task<bool> UpdateImageNameAsync(int id, string imageName, CancellationToken ct);
    }
}
