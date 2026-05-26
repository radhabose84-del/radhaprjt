using System.Data;
using Dapper;
using Contracts.Dtos.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Gate;
using Contracts.Interfaces.Lookups.Users;
using GateEntryManagement.Application.Common.Interfaces.IGatePass;
using GateEntryManagement.Application.GatePass.Dto;

namespace GateEntryManagement.Infrastructure.Repositories.GatePass
{
    public class GatePassQueryRepository : IGatePassQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUnitLookup _unitLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IEnumerable<IGatePassDocResolver> _docResolvers;

        public GatePassQueryRepository(
            IDbConnection dbConnection,
            IUnitLookup unitLookup,
            IIPAddressService ipAddressService,
            IEnumerable<IGatePassDocResolver> docResolvers)
        {
            _dbConnection = dbConnection;
            _unitLookup = unitLookup;
            _ipAddressService = ipAddressService;
            _docResolvers = docResolvers;
        }

        public async Task<(List<GatePassHdrDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var whereClause = "h.IsDeleted = 0 AND h.UnitId = @UnitId";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (h.GatePassNo LIKE @Search OR h.VehicleNumber LIKE @Search OR h.DriverName LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Gate.GatePassHdr h
                WHERE {whereClause};

                SELECT h.Id, h.GatePassNo, h.GatePassDate,
                    h.VehicleMovementRecordId, vmr.VehicleMovementId,
                    h.VehicleNumber, h.DriverName, h.DriverMobile, h.TransporterName,
                    h.UnitId,
                    h.TotalItems, h.TotalDocumentQty, h.TotalDispatchQty,
                    h.ReturnableItems, h.TotalValue,
                    h.GrossWeight, h.TareWeight, h.NetWeight,
                    h.Remarks,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName, h.CreatedIP,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName, h.ModifiedIP
                FROM Gate.GatePassHdr h
                LEFT JOIN Gate.VehicleMovementRecord vmr ON h.VehicleMovementRecordId = vmr.Id AND vmr.IsDeleted = 0
                WHERE {whereClause}
                ORDER BY h.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new { UnitId = unitId, Search = $"%{searchTerm}%", Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<GatePassHdrDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            await PopulateUnitNames(list);

            return (list, totalCount);
        }

        public async Task<GatePassHdrDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT h.Id, h.GatePassNo, h.GatePassDate,
                    h.VehicleMovementRecordId, vmr.VehicleMovementId,
                    h.VehicleNumber, h.DriverName, h.DriverMobile, h.TransporterName,
                    vmr.GateInTime, vmr.GateOutTime,
                    h.UnitId,
                    h.TotalItems, h.TotalDocumentQty, h.TotalDispatchQty,
                    h.ReturnableItems, h.TotalValue,
                    h.GrossWeight, h.TareWeight, h.NetWeight,
                    h.Remarks,
                    h.IsActive, h.IsDeleted,
                    h.CreatedBy, h.CreatedDate, h.CreatedByName, h.CreatedIP,
                    h.ModifiedBy, h.ModifiedDate, h.ModifiedByName, h.ModifiedIP
                FROM Gate.GatePassHdr h
                LEFT JOIN Gate.VehicleMovementRecord vmr ON h.VehicleMovementRecordId = vmr.Id AND vmr.IsDeleted = 0
                WHERE h.Id = @Id AND h.IsDeleted = 0";

            const string detailSql = @"
                SELECT d.Id, d.GatePassHdrId, d.DocTypeId,
                    tt.ShortName AS DocTypeCode, tt.TypeName AS DocTypeName,
                    d.DocId, d.DocNo,
                    d.PartyName, d.PartyCode, d.DocDate, d.TotalQty
                FROM Gate.GatePassDtl d
                LEFT JOIN Finance.TransactionTypeMaster tt ON tt.Id = d.DocTypeId AND tt.IsDeleted = 0
                WHERE d.GatePassHdrId = @Id";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<GatePassHdrDto>(headerSql, new { Id = id });
            if (header == null)
                return null;

            var details = (await _dbConnection.QueryAsync<GatePassDtlDto>(detailSql, new { Id = id })).ToList();
            header.GatePassDetails = details;

            await PopulateUnitNames(new List<GatePassHdrDto> { header });
            await ResolveDocFieldsAsync(header, details);

            return header;
        }

        private async Task ResolveDocFieldsAsync(GatePassHdrDto header, List<GatePassDtlDto> details)
        {
            var groups = details
                .Where(d => !string.IsNullOrWhiteSpace(d.DocTypeName) && d.DocId > 0)
                .GroupBy(d => d.DocTypeName!)
                .ToList();

            if (groups.Count == 0)
                return;

            GatePassDocSummaryDto? firstResolvedSummary = null;

            foreach (var group in groups)
            {
                var resolver = _docResolvers.FirstOrDefault(r => r.DocumentType == group.Key);
                if (resolver == null)
                    continue;

                var docIds = group.Select(d => d.DocId).Distinct().ToList();
                var summaries = await resolver.GetSummariesAsync(docIds);
                if (summaries.Count == 0)
                    continue;

                var dict = summaries.ToDictionary(s => s.DocId);

                foreach (var detail in group)
                {
                    if (!dict.TryGetValue(detail.DocId, out var summary))
                        continue;

                    if (summary.TotalQty.HasValue)
                        detail.TotalQty = summary.TotalQty.Value;

                    detail.NetKgs = summary.NetKgs;
                    detail.GrossKgs = summary.GrossKgs;
                    detail.WithLoadKgs = summary.WithLoadKgs;
                    detail.WithoutLoadKgs = summary.WithoutLoadKgs;
                    detail.TotalWeight = summary.TotalWeight;
                    detail.ItemDescription = summary.ItemDescription;
                    detail.Uom = summary.Uom;

                    if (firstResolvedSummary == null && !string.IsNullOrWhiteSpace(summary.TransporterName))
                        firstResolvedSummary = summary;
                }
            }

            if (string.IsNullOrWhiteSpace(header.TransporterName) && firstResolvedSummary != null)
                header.TransporterName = firstResolvedSummary.TransporterName;
        }

        public async Task<IReadOnlyList<GatePassAutoCompleteDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT h.Id, h.GatePassNo, h.GatePassDate, h.VehicleNumber, h.DriverName
                FROM Gate.GatePassHdr h
                WHERE h.IsDeleted = 0 AND h.IsActive = 1
                AND (h.GatePassNo LIKE @Term OR h.VehicleNumber LIKE @Term OR h.DriverName LIKE @Term)
                ORDER BY h.Id DESC";

            var result = await _dbConnection.QueryAsync<GatePassAutoCompleteDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Gate.GatePassHdr
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> VehicleMovementRecordExistsAsync(int vmrId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Gate.VehicleMovementRecord
                WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = vmrId });
            return count > 0;
        }

        public async Task<bool> UnitExistsAsync(int unitId)
        {
            var unit = await _unitLookup.GetByIdAsync(unitId);
            return unit != null;
        }

        private async Task PopulateUnitNames(List<GatePassHdrDto> items)
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
