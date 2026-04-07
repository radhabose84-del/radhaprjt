using System.Data;
using Contracts.Interfaces.Validations.PurchaseManagement;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.Validations;

internal sealed class PartyMasterPurchaseValidationRepository : IPartyMasterPurchaseValidation
{
    private readonly IDbConnection _dbConnection;

    public PartyMasterPurchaseValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedPartyMasterAsync(int partyId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderHeader] WHERE VendorId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Purchase].[QuotationHeader]  WHERE SupplierId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Purchase].[GateEntryHeader]  WHERE PartyId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Purchase].[PriceMasterHeader] WHERE VendorId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseBillEntryHeader] WHERE PartyId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Purchase].[ServiceEntrySheets] WHERE VendorId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Purchase].[GrnHeader]        WHERE PartyId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[RfqSuppliers]     WHERE SupplierId = @Id)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = partyId });
    }

    public async Task<bool> HasActivePartyMasterAsync(int partyId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Purchase].[PurchaseOrderHeader] WHERE VendorId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Purchase].[QuotationHeader]  WHERE SupplierId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Purchase].[GateEntryHeader]  WHERE PartyId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Purchase].[PriceMasterHeader] WHERE VendorId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Purchase].[PurchaseBillEntryHeader] WHERE PartyId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Purchase].[ServiceEntrySheets] WHERE VendorId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Purchase].[GrnHeader]        WHERE PartyId = @Id)
                OR EXISTS (SELECT 1 FROM [Purchase].[RfqSuppliers]     WHERE SupplierId = @Id)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = partyId });
    }
}
