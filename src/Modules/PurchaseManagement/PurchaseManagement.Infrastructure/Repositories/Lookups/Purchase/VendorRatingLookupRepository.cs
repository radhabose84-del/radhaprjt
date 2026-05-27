using System.Data;
using Contracts.Dtos.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Purchase;
using Dapper;

namespace PurchaseManagement.Infrastructure.Repositories.Lookups.Purchase
{
    internal sealed class VendorRatingLookupRepository : IVendorRatingLookup
    {
        private readonly IDbConnection _dbConnection;

        public VendorRatingLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<VendorRatingLookupDto?> GetLatestRatingByVendorIdAsync(int vendorId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT TOP 1
                    veh.VendorId,
                    veh.EvaluationCode,
                    veh.EvaluationMonth,
                    veh.EvaluationYear,
                    veh.TotalWeightedScore,
                    vrg.GradeName AS GradeName,
                    mm.Description AS StatusName
                FROM Purchase.VendorEvaluationHeader veh
                LEFT JOIN Purchase.VendorRatingGrade vrg ON veh.GradeId = vrg.Id AND vrg.IsDeleted = 0
                LEFT JOIN Purchase.MiscMaster mm ON veh.StatusId = mm.Id AND mm.IsDeleted = 0
                WHERE veh.VendorId = @VendorId AND veh.IsDeleted = 0 AND veh.IsActive = 1
                ORDER BY veh.EvaluationYear DESC, veh.EvaluationMonth DESC";

            return await _dbConnection.QueryFirstOrDefaultAsync<VendorRatingLookupDto>(
                new CommandDefinition(sql, new { VendorId = vendorId }, cancellationToken: ct));
        }
    }
}
