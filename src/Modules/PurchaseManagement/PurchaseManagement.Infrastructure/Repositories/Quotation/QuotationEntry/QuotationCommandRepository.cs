#nullable disable
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationEntry;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;
using System.Data;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.Quotation.QuotationEntry;

public class QuotationCommandRepository(ApplicationDbContext db,IDbConnection dbConnection) : IQuotationCommandRepository
{
    public Task<bool> ExistsForSupplierRfqAsync(int supplierId, int rfqId, CancellationToken ct)
        => db.Set<QuotationHeader>()
             .AnyAsync(x =>
                 x.IsDeleted == BaseEntity.IsDelete.NotDeleted &&
                 x.SupplierId == supplierId &&
                 x.RfqId == rfqId, ct);

    public Task<bool> ExistsForSupplierRfqOtherAsync(int id, int supplierId, int rfqId, CancellationToken ct)
        => db.Set<QuotationHeader>()
             .AnyAsync(x =>
                 x.IsDeleted == BaseEntity.IsDelete.NotDeleted &&
                 x.Id != id &&
                 x.SupplierId == supplierId &&
                 x.RfqId == rfqId, ct);

    public Task AddAsync(QuotationHeader header, CancellationToken ct)
        => db.Set<QuotationHeader>().AddAsync(header, ct).AsTask();

    public Task<QuotationHeader> GetWithLinesAsync(int id, CancellationToken ct)
        => db.Set<QuotationHeader>()
             .Include(h => h.Lines)
             .FirstOrDefaultAsync(h =>
                 h.Id == id &&
                 h.IsDeleted == BaseEntity.IsDelete.NotDeleted, ct);

    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
     public async Task<string> GetBaseDirectoryAsync(CancellationToken ct = default)
        {
            const string query = @"
                SELECT Description AS BaseDirectory
                FROM Purchase.MiscTypeMaster
                WHERE MiscTypeCode = 'QuotationImage' AND IsDeleted = 0";
            var result = await dbConnection.QueryFirstOrDefaultAsync<string>(query);
            return result;
        }

        public async Task<bool> RemoveImageReferenceAsync(string imageName)
        {
            var quotation = await db.QuotationHeaders.FirstOrDefaultAsync(x => x.QuotationImage == imageName);
            if (quotation == null) return false;
            quotation.QuotationImage = null;
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateQuotationImageAsync(int QuotationId, string imageName, CancellationToken ct = default)
        {
            var quotation = await db.QuotationHeaders.FindAsync(QuotationId);
            if (quotation == null) return false;
            quotation.QuotationImage = imageName;
            await db.SaveChangesAsync(ct);
            return true;
        }
}
