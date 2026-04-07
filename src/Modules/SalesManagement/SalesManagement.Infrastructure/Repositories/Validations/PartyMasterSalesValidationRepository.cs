using System.Data;
using Contracts.Interfaces.Validations.SalesManagement;
using Dapper;

namespace SalesManagement.Infrastructure.Repositories.Validations;

internal sealed class PartyMasterSalesValidationRepository : IPartyMasterSalesValidation
{
    private readonly IDbConnection _dbConnection;

    public PartyMasterSalesValidationRepository(IDbConnection dbConnection)
        => _dbConnection = dbConnection;

    public async Task<bool> HasLinkedPartyMasterAsync(int partyId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Sales].[SalesOrderHeader]       WHERE PartyId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesOrderHeader]    WHERE AgentId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesOrderHeader]    WHERE SubAgentId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Sales].[InvoiceHeader]       WHERE PartyId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Sales].[InvoiceHeader]       WHERE AgentId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesQuotationHeader] WHERE CustomerId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesReturnHeader]   WHERE CustomerId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Sales].[DispatchAdviceHeader] WHERE PartyId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesEnquiryHeader]  WHERE PartyId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Sales].[ComplaintHeader]     WHERE CustomerId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Sales].[CustomerVisit]       WHERE CustomerId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Sales].[AgentCommissionConfig] WHERE AgentId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Sales].[AgentCustomerMapping] WHERE AgentId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Sales].[AgentCustomerMapping] WHERE CustomerId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Sales].[AgentCustomerMapping] WHERE SubAgentId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Sales].[DispatchAddressMapping] WHERE PartyId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesContact]        WHERE PartyId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesLead]           WHERE PartyId = @Id AND IsDeleted = 0)
                OR EXISTS (SELECT 1 FROM [Sales].[OfficerAgent]        WHERE AgentId = @Id)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = partyId });
    }

    public async Task<bool> HasActivePartyMasterAsync(int partyId)
    {
        const string sql = @"
            SELECT CASE WHEN
                EXISTS (SELECT 1 FROM [Sales].[SalesOrderHeader]       WHERE PartyId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesOrderHeader]    WHERE AgentId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesOrderHeader]    WHERE SubAgentId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Sales].[InvoiceHeader]       WHERE PartyId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Sales].[InvoiceHeader]       WHERE AgentId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesQuotationHeader] WHERE CustomerId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesReturnHeader]   WHERE CustomerId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Sales].[DispatchAdviceHeader] WHERE PartyId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesEnquiryHeader]  WHERE PartyId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Sales].[ComplaintHeader]     WHERE CustomerId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Sales].[CustomerVisit]       WHERE CustomerId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Sales].[AgentCommissionConfig] WHERE AgentId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Sales].[AgentCustomerMapping] WHERE AgentId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Sales].[AgentCustomerMapping] WHERE CustomerId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Sales].[AgentCustomerMapping] WHERE SubAgentId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Sales].[DispatchAddressMapping] WHERE PartyId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesContact]        WHERE PartyId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Sales].[SalesLead]           WHERE PartyId = @Id AND IsDeleted = 0 AND IsActive = 1)
                OR EXISTS (SELECT 1 FROM [Sales].[OfficerAgent]        WHERE AgentId = @Id AND IsActive = 1)
            THEN 1 ELSE 0 END";

        return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = partyId });
    }
}
