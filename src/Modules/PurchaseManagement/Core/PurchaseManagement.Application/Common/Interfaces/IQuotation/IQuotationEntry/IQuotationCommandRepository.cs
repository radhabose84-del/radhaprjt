using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;

namespace PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationEntry;

public interface IQuotationCommandRepository
{
    Task<bool> ExistsForSupplierRfqAsync(int supplierId, int rfqId, CancellationToken ct);
    Task<bool> ExistsForSupplierRfqOtherAsync(int id, int supplierId, int rfqId, CancellationToken ct); // exclude this Id (for update)
    Task AddAsync(QuotationHeader header, CancellationToken ct);
    Task<QuotationHeader?> GetWithLinesAsync(int id, CancellationToken ct);
    Task SaveAsync(CancellationToken ct);
    Task<string> GetBaseDirectoryAsync(CancellationToken ct = default);
    Task<bool> RemoveImageReferenceAsync(string imagePath);
    Task<bool> UpdateQuotationImageAsync(int quotationId, string imageName, CancellationToken ct = default);    
}
