using System.Data;
using Contracts.Interfaces.Validations.SalesManagement;
using Dapper;

namespace SalesManagement.Infrastructure.Repositories.Validations;

internal sealed class SalesItemValidationRepository : ISalesItemValidation
{
    private readonly IDbConnection _dbConnection;

    public SalesItemValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedItemAsync(int itemId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Sales].[SalesOrderDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesQuotationDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[InvoiceDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[DeliveryChallanDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesEnquiryDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[DispatchAdviceDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesReturnDetail] WHERE ItemId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Sales].[ComplaintDetail] WHERE ItemId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Sales].[ItemPriceMaster] WHERE ItemId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Sales].[StoDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[StoReceiptDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[StockLedger] WHERE ItemId = @Id)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = itemId });
    }

    public async Task<bool> HasActiveItemAsync(int itemId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Sales].[SalesOrderDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesQuotationDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[InvoiceDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[DeliveryChallanDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesEnquiryDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[DispatchAdviceDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesReturnDetail] WHERE ItemId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Sales].[ComplaintDetail] WHERE ItemId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Sales].[ItemPriceMaster] WHERE ItemId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Sales].[StoDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[StoReceiptDetail] WHERE ItemId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[StockLedger] WHERE ItemId = @Id)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = itemId });
    }
}
