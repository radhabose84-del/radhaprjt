using System.Data;
using Dapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Dto;

namespace GateEntryManagement.Infrastructure.Repositories.VehicleMovementRecord
{
    public class VehicleMovementRecordQueryRepository : IVehicleMovementRecordQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPartyLookup _partyLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly IIPAddressService _ipAddressService;

        public VehicleMovementRecordQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup,
            IUnitLookup unitLookup,
            IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
            _unitLookup = unitLookup;
            _ipAddressService = ipAddressService;
        }

        public async Task<(List<VehicleMovementRecordDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var whereClause = "vmr.IsDeleted = 0 AND vmr.UnitId = @UnitId";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (vmr.VehicleMovementId LIKE @Search OR vmr.VehicleNumber LIKE @Search OR vmr.DriverName LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Gate.VehicleMovementRecord vmr
                WHERE {whereClause};

                SELECT vmr.Id, vmr.VehicleMovementId, vmr.VehicleNumber, vmr.DriverName,
                    vmr.DriverLicenseNo, vmr.DriverMobileNo, vmr.TransporterId,
                    vmr.PurposeOfVisitId, pov.Description AS PurposeOfVisitName,
                    vmr.ReferenceDocTypeId, rdt.Description AS ReferenceDocTypeName,
                    vmr.ReferenceDocNo,
                    vmr.GateInTime, vmr.GateInBy, vmr.GateOutTime, vmr.GateOutBy,
                    vmr.StatusId, st.Description AS StatusName,
                    vmr.UnitId, vmr.Remarks,
                    vmr.IsActive, vmr.IsDeleted,
                    vmr.CreatedBy, vmr.CreatedDate, vmr.CreatedByName, vmr.CreatedIP,
                    vmr.ModifiedBy, vmr.ModifiedDate, vmr.ModifiedByName, vmr.ModifiedIP
                FROM Gate.VehicleMovementRecord vmr
                LEFT JOIN Gate.MiscMaster pov ON vmr.PurposeOfVisitId = pov.Id AND pov.IsDeleted = 0
                LEFT JOIN Gate.MiscMaster rdt ON vmr.ReferenceDocTypeId = rdt.Id AND rdt.IsDeleted = 0
                LEFT JOIN Gate.MiscMaster st ON vmr.StatusId = st.Id AND st.IsDeleted = 0
                WHERE {whereClause}
                ORDER BY vmr.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new { UnitId = unitId, Search = $"%{searchTerm}%", Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<VehicleMovementRecordDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            // Populate cross-module lookups
            await PopulateCrossModuleNames(list);

            return (list, totalCount);
        }

        public async Task<VehicleMovementRecordDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT vmr.Id, vmr.VehicleMovementId, vmr.VehicleNumber, vmr.DriverName,
                    vmr.DriverLicenseNo, vmr.DriverMobileNo, vmr.TransporterId,
                    vmr.PurposeOfVisitId, pov.Description AS PurposeOfVisitName,
                    vmr.ReferenceDocTypeId, rdt.Description AS ReferenceDocTypeName,
                    vmr.ReferenceDocNo,
                    vmr.GateInTime, vmr.GateInBy, vmr.GateOutTime, vmr.GateOutBy,
                    vmr.StatusId, st.Description AS StatusName,
                    vmr.UnitId, vmr.Remarks,
                    vmr.IsActive, vmr.IsDeleted,
                    vmr.CreatedBy, vmr.CreatedDate, vmr.CreatedByName, vmr.CreatedIP,
                    vmr.ModifiedBy, vmr.ModifiedDate, vmr.ModifiedByName, vmr.ModifiedIP
                FROM Gate.VehicleMovementRecord vmr
                LEFT JOIN Gate.MiscMaster pov ON vmr.PurposeOfVisitId = pov.Id AND pov.IsDeleted = 0
                LEFT JOIN Gate.MiscMaster rdt ON vmr.ReferenceDocTypeId = rdt.Id AND rdt.IsDeleted = 0
                LEFT JOIN Gate.MiscMaster st ON vmr.StatusId = st.Id AND st.IsDeleted = 0
                WHERE vmr.Id = @Id AND vmr.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<VehicleMovementRecordDto>(sql, new { Id = id });

            if (dto != null)
                await PopulateCrossModuleNames(new List<VehicleMovementRecordDto> { dto });

            return dto;
        }

        public async Task<IReadOnlyList<VehicleMovementRecordAutoCompleteDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT vmr.Id, vmr.VehicleMovementId, vmr.VehicleNumber, vmr.DriverName,
                    st.Description AS StatusName
                FROM Gate.VehicleMovementRecord vmr
                LEFT JOIN Gate.MiscMaster st ON vmr.StatusId = st.Id AND st.IsDeleted = 0
                WHERE vmr.IsDeleted = 0 AND vmr.IsActive = 1
                AND (vmr.VehicleMovementId LIKE @Term OR vmr.VehicleNumber LIKE @Term OR vmr.DriverName LIKE @Term)
                ORDER BY vmr.Id DESC";

            var result = await _dbConnection.QueryAsync<VehicleMovementRecordAutoCompleteDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<List<PendingVehicleDto>> GetPendingVehiclesAsync(
            int unitId, string? vehicleMovementId, string? vehicleNumber, CancellationToken ct)
        {
            var whereClause = @"vmr.IsDeleted = 0 AND vmr.IsActive = 1
                AND vmr.GateOutTime IS NULL
                AND vmr.UnitId = @UnitId
                AND st.Code = @StatusCode";

            if (!string.IsNullOrWhiteSpace(vehicleMovementId))
                whereClause += " AND vmr.VehicleMovementId LIKE @VehicleMovementId";

            if (!string.IsNullOrWhiteSpace(vehicleNumber))
                whereClause += " AND vmr.VehicleNumber LIKE @VehicleNumber";

            var sql = $@"
                SELECT vmr.Id, vmr.VehicleMovementId, vmr.VehicleNumber,
                    vmr.DriverName, vmr.DriverMobileNo, vmr.GateInTime,
                    st.Description AS StatusName
                FROM Gate.VehicleMovementRecord vmr
                LEFT JOIN Gate.MiscMaster st ON vmr.StatusId = st.Id AND st.IsDeleted = 0
                WHERE {whereClause}
                ORDER BY vmr.GateInTime DESC";

            var parameters = new
            {
                UnitId = unitId,
                StatusCode = GateEntryManagement.Domain.Common.MiscEnumEntity.VMRStatusInsidePremises,
                VehicleMovementId = $"%{vehicleMovementId}%",
                VehicleNumber = $"%{vehicleNumber}%"
            };

            var result = await _dbConnection.QueryAsync<PendingVehicleDto>(
                new CommandDefinition(sql, parameters, cancellationToken: ct));

            // Populate TransporterName via cross-module lookup
            var list = result.ToList();
            return list;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Gate.VehicleMovementRecord
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> HasOpenVMRForVehicleAsync(string vehicleNumber)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Gate.VehicleMovementRecord vmr
                INNER JOIN Gate.MiscMaster st ON vmr.StatusId = st.Id AND st.IsDeleted = 0
                WHERE vmr.VehicleNumber = @VehicleNumber
                AND vmr.IsDeleted = 0
                AND vmr.GateOutTime IS NULL";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { VehicleNumber = vehicleNumber.Trim() });
            return count > 0;
        }

        public async Task<bool> MiscMasterExistsAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Gate.MiscMaster
                WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> TransporterExistsAsync(int transporterId)
        {
            var party = await _partyLookup.GetByIdAsync(transporterId);
            return party != null;
        }

        public async Task<bool> UnitExistsAsync(int unitId)
        {
            var unit = await _unitLookup.GetByIdAsync(unitId);
            return unit != null;
        }

        private async Task PopulateCrossModuleNames(List<VehicleMovementRecordDto> items)
        {
            // Transporter names
            var transporterIds = items.Where(x => x.TransporterId.HasValue).Select(x => x.TransporterId!.Value).Distinct();
            if (transporterIds.Any())
            {
                var parties = await _partyLookup.GetByIdsAsync(transporterIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);
                foreach (var item in items.Where(x => x.TransporterId.HasValue))
                {
                    if (partyDict.TryGetValue(item.TransporterId!.Value, out var name))
                        item.TransporterName = name;
                }
            }

            // Unit names
            var unitIds = items.Select(x => x.UnitId).Distinct();
            if (unitIds.Any())
            {
                var units = await _unitLookup.GetByIdsAsync(unitIds);
                var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);
                foreach (var item in items)
                {
                    if (unitDict.TryGetValue(item.UnitId, out var name))
                        item.UnitName = name;
                }
            }
        }
    }
}
