using System.Data;
using Contracts.Interfaces.Validations.PurchaseManagement;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.Validations;

internal sealed class PurchaseUomValidationRepository : IPurchaseUomValidation
{
    private readonly IDbConnection _dbConnection;

    public PurchaseUomValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedUomAsync(int uomId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Purchase].[IndentDetail] WHERE ItemUOMId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseLocalDetail] WHERE UOMId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Purchase].[GrnDetail] WHERE UOMId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[IssueDetail] WHERE UomId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[IssueReturnDetail] WHERE UomId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[MrsDetail] WHERE UomId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[ServiceMaster] WHERE UomId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Purchase].[PriceMasterHeader] WHERE UomId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Purchase].[QuotationDetail] WHERE UomId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Purchase].[RfqItem] WHERE UomId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[StockLedger] WHERE UomId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[SubStoreStockLedger] WHERE UomId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseBillEntryDetail] WHERE UomId = @Id AND IsDeleted = 0)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = uomId });
    }

    public async Task<bool> HasActiveUomAsync(int uomId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Purchase].[IndentDetail] WHERE ItemUOMId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseLocalDetail] WHERE UOMId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Purchase].[GrnDetail] WHERE UOMId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[IssueDetail] WHERE UomId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[IssueReturnDetail] WHERE UomId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[MrsDetail] WHERE UomId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[ServiceMaster] WHERE UomId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Purchase].[PriceMasterHeader] WHERE UomId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Purchase].[QuotationDetail] WHERE UomId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Purchase].[RfqItem] WHERE UomId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[StockLedger] WHERE UomId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[SubStoreStockLedger] WHERE UomId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseBillEntryDetail] WHERE UomId = @Id AND IsDeleted = 0 AND IsActive = 1)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = uomId });
    }
}
