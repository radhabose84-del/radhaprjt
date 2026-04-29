namespace SalesManagement.Application.Common.Interfaces.ISalesOrderTypeMaster
{
    public interface ISalesOrderTypeMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.SalesOrderTypeMaster entity);
        Task<int> UpdateAsync(Domain.Entities.SalesOrderTypeMaster entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken);
    }
}
