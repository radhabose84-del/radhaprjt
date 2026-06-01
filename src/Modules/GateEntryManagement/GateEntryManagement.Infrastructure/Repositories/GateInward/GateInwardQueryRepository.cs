using System.Data;
using Dapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using GateEntryManagement.Application.Common.Interfaces.IGateInward;
using GateEntryManagement.Application.GateInward.Dto;
using GateEntryManagement.Domain.Common;

namespace GateEntryManagement.Infrastructure.Repositories.GateInward
{
    public class GateInwardQueryRepository : IGateInwardQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUnitLookup _unitLookup;
        private readonly IPartyLookup _partyLookup;
        private readonly IIPAddressService _ipAddressService;

        public GateInwardQueryRepository(
            IDbConnection dbConnection,
            IUnitLookup unitLookup,
            IPartyLookup partyLookup,
            IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _unitLookup = unitLookup;
            _partyLookup = partyLookup;
            _ipAddressService = ipAddressService;
        }

        public async Task<(List<GateInwardHdrDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var whereClause = "h.IsDeleted = 0 AND h.UnitId = @UnitId";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (h.GateEntryNo LIKE @Search OR vmr.VehicleNumber LIKE @Search OR vmr.DriverName LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Gate.GateInwardHdr h
                LEFT JOIN Gate.VehicleMovementRecord vmr ON h.VehicleMovementRecordId = vmr.Id
                WHERE {whereClause};

                SELECT h.Id, h.GateEntryNo,
                    h.VehicleMovementRecordId, vmr.VehicleMovementId, vmr.VehicleNumber, vmr.DriverName,
                    h.PartyId,
                    h.GrossWeight, h.TareWeight, h.NetWeight,
                    h.InvoiceNo, h.InvoiceDate, h.DcNo, h.DcDate,
                    h.ReceivingWarehouseId,
                    h.QAInspectionRequired, h.QAStatusId, qa.Description AS QAStatusName,
                    h.UnitId, h.Remarks,
                    h.ReceivingTypeId, rt.Description AS ReceivingTypeName,
                    h.CourierNumber,
                    h.AttachmentFileName, h.AttachmentFilePath,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName, h.CreatedIP,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName, h.ModifiedIP
                FROM Gate.GateInwardHdr h
                LEFT JOIN Gate.VehicleMovementRecord vmr ON h.VehicleMovementRecordId = vmr.Id AND vmr.IsDeleted = 0
                LEFT JOIN Gate.MiscMaster qa ON h.QAStatusId = qa.Id AND qa.IsDeleted = 0
                LEFT JOIN Gate.MiscMaster rt ON h.ReceivingTypeId = rt.Id AND rt.IsDeleted = 0
                WHERE {whereClause}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new { UnitId = unitId, Search = $"%{searchTerm}%", Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<GateInwardHdrDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            await PopulateUnitNames(list);
            await PopulatePartyNamesAsync(list);
            await ComposeAttachmentUrlsAsync(list);
            return (list, totalCount);
        }

        public async Task<GateInwardHdrDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT h.Id, h.GateEntryNo,
                    h.VehicleMovementRecordId, vmr.VehicleMovementId, vmr.VehicleNumber, vmr.DriverName,
                    h.PartyId,
                    h.GrossWeight, h.TareWeight, h.NetWeight,
                    h.InvoiceNo, h.InvoiceDate, h.DcNo, h.DcDate,
                    h.ReceivingWarehouseId,
                    h.QAInspectionRequired, h.QAStatusId, qa.Description AS QAStatusName,
                    h.UnitId, h.Remarks,
                    h.ReceivingTypeId, rt.Description AS ReceivingTypeName,
                    h.CourierNumber,
                    h.AttachmentFileName, h.AttachmentFilePath,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName, h.CreatedIP,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName, h.ModifiedIP
                FROM Gate.GateInwardHdr h
                LEFT JOIN Gate.VehicleMovementRecord vmr ON h.VehicleMovementRecordId = vmr.Id AND vmr.IsDeleted = 0
                LEFT JOIN Gate.MiscMaster qa ON h.QAStatusId = qa.Id AND qa.IsDeleted = 0
                LEFT JOIN Gate.MiscMaster rt ON h.ReceivingTypeId = rt.Id AND rt.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0";

            const string detailSql = @"
                SELECT d.Id, d.GateInwardHdrId, d.ReferenceDocTypeId, d.ReferenceDocNo, d.PartyName,
                    d.PoId, d.PoSlNoLocal, d.DcQuantity
                FROM Gate.GateInwardDtl d
                WHERE d.GateInwardHdrId = @Id";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<GateInwardHdrDto>(headerSql, new { Id = id });
            if (header == null) return null;

            var details = (await _dbConnection.QueryAsync<GateInwardDtlDto>(detailSql, new { Id = id })).ToList();
            header.GateInwardDetails = details;

            await PopulateUnitNames(new List<GateInwardHdrDto> { header });
            await PopulatePartyNamesAsync(new List<GateInwardHdrDto> { header });
            await ComposeAttachmentUrlsAsync(new List<GateInwardHdrDto> { header });
            return header;
        }

        public async Task<IReadOnlyList<GateInwardAutoCompleteDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT h.Id, h.GateEntryNo, vmr.VehicleNumber, vmr.DriverName
                FROM Gate.GateInwardHdr h
                LEFT JOIN Gate.VehicleMovementRecord vmr ON h.VehicleMovementRecordId = vmr.Id AND vmr.IsDeleted = 0
                WHERE h.IsDeleted = 0 AND h.IsActive = 1
                AND (h.GateEntryNo LIKE @Term OR vmr.VehicleNumber LIKE @Term OR vmr.DriverName LIKE @Term)
                ORDER BY h.Id DESC";

            var result = await _dbConnection.QueryAsync<GateInwardAutoCompleteDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Gate.GateInwardHdr WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> VehicleMovementRecordExistsAsync(int vmrId)
        {
            const string sql = "SELECT COUNT(1) FROM Gate.VehicleMovementRecord WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = vmrId });
            return count > 0;
        }

        public async Task<bool> UnitExistsAsync(int unitId)
        {
            var unit = await _unitLookup.GetByIdAsync(unitId);
            return unit != null;
        }

        public async Task<bool> MiscMasterExistsAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM Gate.MiscMaster WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> IsCourierReceivingTypeAsync(int miscId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Gate.MiscMaster mm
                INNER JOIN Gate.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id
                WHERE mm.Id = @Id
                  AND mt.MiscTypeCode = @MiscTypeCode
                  AND mm.Description = @Description
                  AND mm.IsDeleted = 0 AND mm.IsActive = 1
                  AND mt.IsDeleted = 0 AND mt.IsActive = 1";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new
            {
                Id = miscId,
                MiscTypeCode = MiscEnumEntity.ReceivingType,
                Description = MiscEnumEntity.ReceivingTypeCourier
            });
            return count > 0;
        }

        public async Task<Dictionary<string, string>> GetDocumentDirectoryPath()
        {
            const string sql = @"
                SELECT MiscTypeCode, Description
                FROM Gate.MiscTypeMaster
                WHERE MiscTypeCode IN @MiscTypeCodes
                  AND IsActive = 1 AND IsDeleted = 0;";

            var miscCodes = new[] { MiscEnumEntity.ImagePath, MiscEnumEntity.GateEntryImage };

            var result = await _dbConnection.QueryAsync<(string MiscTypeCode, string Description)>(
                sql, new { MiscTypeCodes = miscCodes });

            return result
                .GroupBy(x => x.MiscTypeCode)
                .ToDictionary(g => g.Key, g => g.First().Description);
        }

        // Composes the web preview URL: {ImagePath}{GateEntryImage}/{AttachmentFileName}
        private async Task ComposeAttachmentUrlsAsync(IEnumerable<GateInwardHdrDto> items)
        {
            var list = items.Where(i => !string.IsNullOrWhiteSpace(i.AttachmentFileName)).ToList();
            if (list.Count == 0) return;

            var dirs = await GetDocumentDirectoryPath();
            var baseUrl = dirs.GetValueOrDefault(MiscEnumEntity.ImagePath, string.Empty);
            var folder = dirs.GetValueOrDefault(MiscEnumEntity.GateEntryImage, string.Empty);

            foreach (var item in list)
                item.AttachmentFilePath = $"{baseUrl}{folder}/{item.AttachmentFileName}";
        }

        private async Task PopulatePartyNamesAsync(List<GateInwardHdrDto> items)
        {
            var partyIds = items
                .Where(x => x.PartyId.HasValue)
                .Select(x => x.PartyId!.Value)
                .Distinct()
                .ToList();
            if (partyIds.Count == 0) return;

            var parties = await _partyLookup.GetByIdsAsync(partyIds);
            var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);
            foreach (var item in items)
            {
                if (item.PartyId.HasValue && partyDict.TryGetValue(item.PartyId.Value, out var name))
                    item.PartyName = name;
            }
        }

        private async Task PopulateUnitNames(List<GateInwardHdrDto> items)
        {
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
