using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.Application.Common.Interfaces.IPaymentTermMaster
{
    public interface IPaymentTermMasterCommandRepository
    {
        Task<int> CreateAsync(PurchaseManagement.Domain.Entities.PaymentTermMaster entity, CancellationToken ct);


        Task<bool> UpdateAsync(PurchaseManagement.Domain.Entities.PaymentTermMaster paymentTermMaster, List<PaymentTermInstallment>? newInstallments);
        
         Task<bool> DeleteAsync(int id);
    }
}