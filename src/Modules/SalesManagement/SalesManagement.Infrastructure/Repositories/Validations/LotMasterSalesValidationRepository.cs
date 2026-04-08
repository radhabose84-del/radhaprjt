using System.Data;
using Contracts.Interfaces.Validations.SalesManagement;
using Dapper;

namespace SalesManagement.Infrastructure.Repositories.Validations;

internal sealed class LotMasterSalesValidationRepository : ILotMasterSalesValidation
{
    private readonly IDbConnection _dbConnection;

    public LotMasterSalesValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedLotMasterAsync(int lotMasterId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Sales].[DeliveryChallanDetail] WHERE LotId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[DispatchAdviceDetail] WHERE LotId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[InvoiceDetail] WHERE LotId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[StoReceiptDetail] WHERE LotId = @Id)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = lotMasterId });
    }

    public async Task<bool> HasActiveLotMasterAsync(int lotMasterId)
    {
        // Detail tables have no IsActive/IsDeleted — any record means active link
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Sales].[DeliveryChallanDetail] WHERE LotId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[DispatchAdviceDetail] WHERE LotId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[InvoiceDetail] WHERE LotId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[StoReceiptDetail] WHERE LotId = @Id)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = lotMasterId });
    }
}
