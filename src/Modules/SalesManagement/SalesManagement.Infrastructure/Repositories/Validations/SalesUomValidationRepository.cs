using System.Data;
using Contracts.Interfaces.Validations.SalesManagement;
using Dapper;

namespace SalesManagement.Infrastructure.Repositories.Validations;

internal sealed class SalesUomValidationRepository : ISalesUomValidation
{
    private readonly IDbConnection _dbConnection;

    public SalesUomValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedUomAsync(int uomId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Sales].[SalesOrderDetail] WHERE SaleUOMId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[InvoiceDetail] WHERE UOMId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[DeliveryChallanDetail] WHERE UOMId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[StoDetail] WHERE UOMId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[StoReceiptDetail] WHERE UOMId = @Id)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = uomId });
    }

    public async Task<bool> HasActiveUomAsync(int uomId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Sales].[SalesOrderDetail] WHERE SaleUOMId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[InvoiceDetail] WHERE UOMId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[DeliveryChallanDetail] WHERE UOMId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[StoDetail] WHERE UOMId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[StoReceiptDetail] WHERE UOMId = @Id)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = uomId });
    }
}
