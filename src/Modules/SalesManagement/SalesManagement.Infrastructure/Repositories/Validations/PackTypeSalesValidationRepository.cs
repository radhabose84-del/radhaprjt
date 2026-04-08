using System.Data;
using Contracts.Interfaces.Validations.SalesManagement;
using Dapper;

namespace SalesManagement.Infrastructure.Repositories.Validations;

internal sealed class PackTypeSalesValidationRepository : IPackTypeSalesValidation
{
    private readonly IDbConnection _dbConnection;

    public PackTypeSalesValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedPackTypeAsync(int packTypeId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Sales].[DispatchAdviceDetail] WHERE PackTypeId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[InvoiceDetail] WHERE PackTypeId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesOrderDetail] WHERE PackTypeId = @Id)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = packTypeId });
    }

    public async Task<bool> HasActivePackTypeAsync(int packTypeId)
    {
        // Detail tables have no IsActive/IsDeleted — any record means active link
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Sales].[DispatchAdviceDetail] WHERE PackTypeId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[InvoiceDetail] WHERE PackTypeId = @Id)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesOrderDetail] WHERE PackTypeId = @Id)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = packTypeId });
    }
}
