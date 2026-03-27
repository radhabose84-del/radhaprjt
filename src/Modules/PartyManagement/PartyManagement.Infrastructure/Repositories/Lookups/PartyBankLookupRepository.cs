using System.Data;
using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Party;
using Dapper;

namespace PartyManagement.Infrastructure.Repositories.Lookups
{
    internal sealed class PartyBankLookupRepository : IPartyBankLookup
    {
        private readonly IDbConnection _dbConnection;

        public PartyBankLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<PartyBankLookupDto?> GetDefaultBankByGstAsync(string gstNumber, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT TOP 1 pb.BankName, pb.BankAccountNumber, pb.BankBranch, pb.IFSCCode
                FROM Party.PartyBank pb
                INNER JOIN Party.PartyMaster pm ON pb.PartyId = pm.Id AND pm.IsDeleted = 0
                WHERE pm.GSTNumber = @GstNumber
                    AND (pb.IsDefaultAccount = 1 OR pb.IsPrimaryAccount = 1)";

            return await _dbConnection.QueryFirstOrDefaultAsync<PartyBankLookupDto>(
                new CommandDefinition(sql, new { GstNumber = gstNumber }, cancellationToken: ct));
        }
    }
}
