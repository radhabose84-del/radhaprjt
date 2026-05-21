using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ServicePO
{
    public interface IServicePurchaseOrderCommandRepository
    {

        Task<PurchaseOrderHeader?> GetAggregateAsync(int id, CancellationToken ct);
        Task<int> AmendAsync(PurchaseOrderHeader existing, PurchaseOrderHeader revised, CancellationToken ct);
        // transactionTypeId — when set, the repository advances Finance.DocumentSequence.DocNo
        // for that TransactionType inside the same DB transaction as the PO insert. Pass null
        // only for callers that don't use IDocumentSequenceLookup-based numbering.
        Task<int> CreateAsync(PurchaseOrderHeader aggregate, CancellationToken ct, int? transactionTypeId = null);

        Task<bool> UpdateServicePOApproveAsync(int id, int statusId, CancellationToken ct = default);

        Task<bool> UpdateAsync(PurchaseOrderHeader aggregate, CancellationToken ct);


        /// <ServiceEntrySheet>

        Task<ServiceEntrySheet> CreateServiceEntrySheetAsync(ServiceEntrySheet entity, CancellationToken ct = default);

        Task<bool> ServiceEntrySheetExistsAsync(int purchaseOrderId, int? serviceScheduleId, CancellationToken ct = default);

        Task<ServiceEntrySheet> UpdateServiceEntrySheetAsync(ServiceEntrySheet aggregate, CancellationToken ct = default);

        Task<ServiceEntrySheet?> GetServiceEntrySheetByIdAsync(int id, CancellationToken ct = default);

        Task<bool> UpdateServiceEntrySheetApproveAsync(int id, int statusId, CancellationToken ct = default);

        
        
        
        
        

       
       
        
     
       









    }
}