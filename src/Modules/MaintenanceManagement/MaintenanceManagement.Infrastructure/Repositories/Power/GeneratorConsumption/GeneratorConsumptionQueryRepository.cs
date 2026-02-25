using System.Data;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.Power.IGeneratorConsumption;
using MaintenanceManagement.Application.Power.GeneratorConsumption.Queries.GetClosingEnergyReaderValueById;
using MaintenanceManagement.Application.Power.GeneratorConsumption.Queries.GetGeneratorConsumption;
using MaintenanceManagement.Application.Power.GeneratorConsumption.Queries.GetUnitIdBasedOnMachineId;
using Dapper;

namespace MaintenanceManagement.Infrastructure.Repositories.Power.GeneratorConsumption
{
    public class GeneratorConsumptionQueryRepository : IGeneratorConsumptionQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;
          public GeneratorConsumptionQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }

        public async Task<(List<GetGeneratorConsumptionDto>, int)> GetAllGeneratorConsumptionAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
             var UnitId = _ipAddressService.GetUnitId();
            var query = $$"""
            DECLARE @TotalCount INT;

            SELECT @TotalCount = COUNT(*)
            FROM Maintenance.GeneratorConsumption a
            INNER JOIN Maintenance.MachineMaster b ON a.GeneratorId = b.Id AND a.UnitId = b.UnitId
            INNER JOIN Maintenance.MiscMaster c ON a.PurposeId = c.Id
            WHERE a.UnitId = @UnitId AND a.IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (b.MachineCode LIKE @Search OR b.MachineName LIKE @Search OR c.Description LIKE @Search)")}};
            
            SELECT 
                a.Id,
                b.MachineCode,
                b.MachineName,
                a.StartTime,
                a.EndTime,
                a.RunningHours,
                a.DieselConsumption,
                a.UnitId,
                a.OpeningEnergyReading,
                a.ClosingEnergyReading,
                a.Energy,
                c.Description AS Purpose,
                a.CreatedByName,
                a.CreatedDate
            FROM Maintenance.GeneratorConsumption a
            INNER JOIN Maintenance.MachineMaster b ON a.GeneratorId = b.Id AND a.UnitId = b.UnitId
            INNER JOIN Maintenance.MiscMaster c ON a.PurposeId = c.Id
            WHERE a.UnitId = @UnitId AND a.IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (b.MachineCode LIKE @Search OR b.MachineName LIKE @Search OR c.Description LIKE @Search)")}}
            ORDER BY a.CreatedDate DESC
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

            var generatorConsumptionList = (await result.ReadAsync<GetGeneratorConsumptionDto>()).ToList();
            int totalCount = await result.ReadFirstAsync<int>();

            return (generatorConsumptionList, totalCount);
        }

        public async Task<List<GetMachineIdBasedonUnitDto>> GetMachineIdBasedonUnit()
        {
            var UnitId = _ipAddressService.GetUnitId();
            const string query = @"
                SELECT 
                B.Id,
                B.MachineCode,
                B.MachineName
            FROM 
                Maintenance.MachineGroup A
            INNER JOIN 
                Maintenance.MachineMaster B 
                ON A.Id = B.MachineGroupId 
                AND A.UnitId = B.UnitId
            WHERE 
                A.PowerSource = 1
                AND A.IsDeleted = 0
                AND B.IsDeleted = 0
                AND A.UnitId = @UnitId;";

            var result = await _dbConnection.QueryAsync<GetMachineIdBasedonUnitDto>(query, new { UnitId = UnitId });

            return result.ToList();
        }

        public async Task<GetClosingEnergyReaderValueDto> GetOpeningReaderValueById(int generatorId)
        {
            var UnitId = _ipAddressService.GetUnitId();
            const string query = @"
                SELECT 
                    f.Id AS GeneratorId,
                    f.MachineCode as GeneratorCode,
                    f.MachineName as GeneratorName,
                    ISNULL(
                        (
                            SELECT TOP 1 pc.ClosingEnergyReading
                            FROM Maintenance.GeneratorConsumption pc
                            WHERE pc.GeneratorId = f.Id AND pc.IsDeleted = 0 And pc.UnitId = @UnitId
                            ORDER BY pc.CreatedDate DESC
                        ),
                        f.ProductionCapacity
                    ) AS OpeningEnergyReading
                FROM Maintenance.MachineMaster f
                WHERE f.Id = @GeneratorId AND f.IsDeleted = 0 AND f.UnitId = @UnitId;";

                var result = await _dbConnection.QueryFirstOrDefaultAsync<GetClosingEnergyReaderValueDto>(query, new { GeneratorId = generatorId, UnitId = UnitId });

            if (result == null)
                throw new Exception($"Generator with ID {generatorId} not found.");

            return result;
        }
    }
}