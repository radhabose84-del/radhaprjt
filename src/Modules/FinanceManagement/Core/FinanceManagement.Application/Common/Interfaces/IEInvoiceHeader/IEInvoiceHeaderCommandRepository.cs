namespace FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader
{
    public interface IEInvoiceHeaderCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.EInvoiceHeader entity);
        Task<int> UpdateAsync(Domain.Entities.EInvoiceHeader entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
