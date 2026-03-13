using System.Data;
using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Party;
using Dapper;

namespace PartyManagement.Infrastructure.Repositories.Lookups
{
    internal sealed class CustomerLookupRepository : ICustomerLookup
    {
        private readonly IDbConnection _dbConnection;

        public CustomerLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<CustomerLookupDto>> GetAllCustomerAsync()
        {
            const string sql = @"
                SELECT DISTINCT p.Id, p.PartyCode AS CustomerCode, p.PartyName AS CustomerName
                FROM Party.PartyMaster p
                INNER JOIN Party.PartyType pt ON pt.PartyId = p.Id
                INNER JOIN Party.MiscMaster mm ON pt.PartyTypeId = mm.Id
                WHERE mm.Description = 'CUSTOMER'
                  AND p.IsActive = 1
                  AND p.IsDeleted = 0
                ORDER BY p.PartyName ASC;";

            var result = await _dbConnection.QueryAsync<CustomerLookupDto>(sql);
            return result.ToList();
        }
    }
}
