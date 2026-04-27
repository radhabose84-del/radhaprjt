using System.Data;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
using PurchaseManagement.Domain.Common;
using Dapper;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;

namespace PurchaseManagement.Infrastructure.Repositories.PurchaseOrder
{
    public class PODocumentQueryRepository : IPODocumentQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ApplicationDbContext _applicationDbContext;

        public PODocumentQueryRepository(
            IDbConnection dbConnection,
            ApplicationDbContext applicationDbContext)
        {
            _dbConnection = dbConnection;
            _applicationDbContext = applicationDbContext;
        }

        public async Task<string> GetDocumentDirectoryAsync()
        {
            const string query = @"
                SELECT TOP 1 Description
                FROM Purchase.MiscTypeMaster
                WHERE MiscTypeCode = @MiscTypeCode
                  AND IsDeleted = 0
                  AND IsActive = 1
                ORDER BY Id DESC";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<string>(
                query,
                new { MiscTypeCode = MiscEnumEntity.DocumentPath }  // "PoDocument"
            );

            return result ?? string.Empty;
        }

        public async Task<bool> DeleteFileDetailsDocumentAsync(int id, int poId, string fileName)
        {
            var entity = await _applicationDbContext.PurchaseDocuments
                .FirstOrDefaultAsync(x => x.Id == id && x.PoId == poId && x.FileName == fileName);

            if (entity == null)
                return false;

            _applicationDbContext.PurchaseDocuments.Remove(entity);
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }

        public async Task<string> GetBaseDirectoryAsync()
        {
            const string query = @"
                SELECT Description AS BaseDirectory
                FROM Budget.MiscTypeMaster
                WHERE MiscTypeCode = @Code AND IsDeleted = 0";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<string>(
                query,
                new { Code = MiscEnumEntity.DocumentPath }
            );

            return result ?? string.Empty;
        }

        public async Task<IReadOnlyCollection<int>> GetPODocumentIdsAsync(int poId)
        {
            if (poId <= 0) return Array.Empty<int>();

            try
            {
                var ids = await _applicationDbContext.PurchaseDocuments
                    .AsNoTracking()
                    .Where(d => d != null && d.PoId == poId && d.DocumentId > 0)
                    .Select(d => d.DocumentId)
                    .Distinct()
                    .ToListAsync();

                return ids;
            }
            catch
            {
                // Defensive: if anything goes wrong, don't crash the caller — report "none".
                return Array.Empty<int>();
            }
        }
    }
}
