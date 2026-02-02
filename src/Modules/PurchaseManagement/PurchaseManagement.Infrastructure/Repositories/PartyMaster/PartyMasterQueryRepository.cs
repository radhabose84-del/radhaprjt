using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces.IPartyMaster;
using PurchaseManagement.Application.PartyMaster.Queries.GetPartyDetails;
using Dapper;
using Serilog;

namespace PurchaseManagement.Infrastructure.Repositories.PartyMaster
{
    public class PartyMasterQueryRepository : IPartyMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        public PartyMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<List<PartyMasterDTO>> GetPartyMasters(string OldunitCode, string searchPattern)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@OldUnitcode", OldunitCode);
            parameters.Add("@SearchTerm", string.IsNullOrWhiteSpace(searchPattern) ? null : searchPattern);
            var result = await _dbConnection.QueryAsync<PartyMasterDTO>(
                "dbo.GetCustomerDetailsByOldUnitcode",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (!result.Any())
            {
            Log.Information("No data returned from stored procedure!");
            }

            return result.ToList()?? new List<PartyMasterDTO>();
        }
    }
}