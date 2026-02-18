#nullable disable
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.Power.IPowerConsumption;
using MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetClosingReaderValueById;
using MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetFeederSubFeederById;
using MaintenanceManagement.Application.Power.PowerConsumption.Queries.GetPowerConsumption;
using Dapper;

namespace MaintenanceManagement.Infrastructure.Repositories.Power.PowerConsumption
{
    public class PowerConsumptionQueryRepository : IPowerConsumptionQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;

        public PowerConsumptionQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }

        public async Task<(List<GetPowerConsumptionDto>, int)> GetAllPowerConsumptionAsync(int PageNumber, int PageSize, string SearchTerm)
        {
            var UnitId = _ipAddressService.GetUnitId();
            var query = $$"""
            DECLARE @TotalCount INT;
            SELECT @TotalCount = COUNT(*) 
            FROM [Maintenance].[PowerConsumption] FG INNER JOIN [Maintenance].[MiscMaster] MM ON FG.FeederTypeId=MM.Id INNER JOIN [Maintenance].[Feeder] FF ON FG.FeederId=FF.Id
            WHERE FG.IsDeleted = 0 AND FG.UnitId = @UnitId
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (MM.description LIKE @Search OR FF.FeederName LIKE @Search)")}}; 

            SELECT FG.Id, FG.FeederTypeId,MM.description as FeederType,FF.FeederName ,FG.FeederId,FG.UnitId,FG.OpeningReading, FG.ClosingReading, 
            FG.TotalUnits, FG.IsActive, FG.CreatedDate, FG.CreatedByName
            FROM [Maintenance].[PowerConsumption] FG INNER JOIN [Maintenance].[MiscMaster] MM ON FG.FeederTypeId=MM.Id INNER JOIN [Maintenance].[Feeder] FF ON FG.FeederId=FF.Id WHERE FG.IsDeleted = 0 AND FG.UnitId = @UnitId
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (MM.description LIKE @Search OR FF.FeederName LIKE @Search)")}}
            ORDER BY FG.CreatedDate DESC 
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT @TotalCount AS TotalCount;
            """;

            var parameters = new
            {
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize,
                UnitId
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);

            var powerConsumptionList = (await result.ReadAsync<GetPowerConsumptionDto>()).ToList();
            int totalCount = await result.ReadFirstAsync<int>();

            return (powerConsumptionList, totalCount);
        }

        public async Task<List<GetFeederSubFeederDto>> GetFeederSubFeedersById(int feederTypeId)
        {
            var UnitId = _ipAddressService.GetUnitId();
            const string query = @"
                SELECT 
                    a.Id, 
                    a.FeederCode, 
                    a.FeederName 
                FROM Maintenance.Feeder a 
                INNER JOIN Maintenance.MiscMaster b 
                    ON a.FeederTypeId = b.Id 
                WHERE a.IsDeleted = 0 
                    AND a.FeederTypeId = @FeederTypeId 
                    AND a.UnitId = @UnitId;";

            var result = await _dbConnection.QueryAsync<GetFeederSubFeederDto>(query, new { FeederTypeId = feederTypeId, UnitId = UnitId });

            return result.ToList();
        }

        public async Task<GetClosingReaderValueDto> GetOpeningReaderValueById(int feederId)
        {
            var UnitId = _ipAddressService.GetUnitId();
            const string query = @"
                SELECT 
                    f.Id AS FeederId,
                    f.FeederCode,
                    f.FeederName,
                    ISNULL(
                        (
                            SELECT TOP 1 pc.ClosingReading
                            FROM Maintenance.PowerConsumption pc
                            WHERE pc.FeederId = f.Id AND pc.IsDeleted = 0 And pc.UnitId = @UnitId
                            ORDER BY pc.CreatedDate DESC
                        ),
                        f.OpeningReading
                    ) AS OpeningReading
                FROM Maintenance.Feeder f
                WHERE f.Id = @FeederId AND f.IsDeleted = 0 AND f.UnitId = @UnitId;";

                var result = await _dbConnection.QueryFirstOrDefaultAsync<GetClosingReaderValueDto>(query, new { FeederId = feederId, UnitId = UnitId });

            if (result == null)
                throw new Exception($"Feeder with ID {feederId} not found.");

            return result;
        }

        public async Task<GetPowerConsumptionDto> GetPowerConsumptionById(int id)
        {
            var UnitId = _ipAddressService.GetUnitId();
             var query = """
                SELECT FG.Id, FG.FeederTypeId, FG.FeederId,FG.UnitId,FG.OpeningReading, FG.ClosingReading, 
                FG.TotalUnits, FG.IsActive, FG.CreatedDate, FG.CreatedByName, FG.ModifiedDate,FG.ModifiedByName
                FROM [Maintenance].[PowerConsumption] FG
                WHERE FG.IsDeleted = 0 AND FG.Id = @Id AND FG.UnitId = @UnitId;
                """;

            var result = await _dbConnection.QueryAsync<GetPowerConsumptionDto>(query, new { Id = id, UnitId = UnitId });
            return result.FirstOrDefault();
        }
    }
}