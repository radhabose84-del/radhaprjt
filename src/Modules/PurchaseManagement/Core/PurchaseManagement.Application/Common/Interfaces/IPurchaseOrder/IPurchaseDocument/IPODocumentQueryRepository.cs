using PurchaseManagement.Domain.PurchaseOrder;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument
{
    public interface IPODocumentQueryRepository
    {
        Task<string> GetDocumentDirectoryAsync();
        Task<bool> DeleteFileDetailsDocumentAsync(int Id, int PartyId, string filename);
        Task<string> GetBaseDirectoryAsync();
         //Task<string> GetBaseDirectoryAsync(CancellationToken ct = default);
         Task<IReadOnlyCollection<int>> GetPODocumentIdsAsync(int poId);        
    }
}
