using System.Data;
using Contracts.Interfaces.Validations.PurchaseManagement;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.Validations;

internal sealed class PurchaseItemValidationRepository : IPurchaseItemValidation
{
    private readonly IDbConnection _dbConnection;

    public PurchaseItemValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedItemAsync(int itemId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Purchase].[IndentDetail] WHERE ItemId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseLocalDetail] WHERE ItemId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Purchase].[GrnDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[IssueDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[IssueReturnDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[MrsDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[PriceMasterHeader] WHERE ItemId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Purchase].[QuotationDetail] WHERE ItemId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Purchase].[RfqItem] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[StockLedger] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[SubStoreStockLedger] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[GrnPutAwayRule] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseBillEntryDetail] WHERE ItemId = @Id AND IsDeleted = 0)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = itemId });
    }

    public async Task<bool> HasActiveItemAsync(int itemId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Purchase].[IndentDetail] WHERE ItemId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseLocalDetail] WHERE ItemId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Purchase].[GrnDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[IssueDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[IssueReturnDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[MrsDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[PriceMasterHeader] WHERE ItemId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Purchase].[QuotationDetail] WHERE ItemId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Purchase].[RfqItem] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[StockLedger] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[SubStoreStockLedger] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[GrnPutAwayRule] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseBillEntryDetail] WHERE ItemId = @Id AND IsDeleted = 0 AND IsActive = 1)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = itemId });
    }
}
