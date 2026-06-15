namespace SalesManagement.Application.Common.Interfaces.ISalesLead
{
    public interface ISalesLeadCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.SalesLead entity, int transactionTypeId, Domain.Entities.SalesContact? newContact = null);
        Task<int> UpdateAsync(Domain.Entities.SalesLead entity);
        Task<int> CloseAsync(Domain.Entities.SalesLead entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
