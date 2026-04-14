namespace SalesManagement.Application.Common.Interfaces.IProformaInvoice
{
    public interface IProformaInvoiceCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.ProformaInvoice entity, int transactionTypeId);
        Task<int> UpdateAsync(Domain.Entities.ProformaInvoice entity);
        Task<int> UpdatePaymentAsync(int id, decimal paymentReceivedAmount, int? statusId);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
