using System.Data;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Dto;

namespace FinanceManagement.Infrastructure.Repositories.EWaybillHeader
{
    public class EWaybillHeaderQueryRepository : IEWaybillHeaderQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IUnitLookup _unitLookup;
        private readonly IPartyLookup _partyLookup;
        private readonly IIPAddressService _ipAddressService;

        public EWaybillHeaderQueryRepository(IDbConnection dbConnection, IUnitLookup unitLookup, IPartyLookup partyLookup, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _unitLookup = unitLookup;
            _partyLookup = partyLookup;
            _ipAddressService = ipAddressService;
        }

        public async Task<(List<EWaybillHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;
            var search = string.IsNullOrWhiteSpace(searchTerm) ? null : $"%{searchTerm}%";
            var unitId = _ipAddressService.GetUnitId();
            var unitFilter = unitId.HasValue ? "AND UnitId = @UnitId" : "";

            var countSql = $@"
                SELECT COUNT(*) FROM [Finance].[EWaybillHeader]
                WHERE IsDeleted = 0
                {unitFilter}
                AND (@Search IS NULL OR EWBNumber LIKE @Search OR InvoiceNo LIKE @Search)";

            var dataSql = $@"
                SELECT Id, EInvoiceHeaderId, UnitId, EWBNumber, InvoiceNo, InvoiceDate, InvoiceValue,
                       SupplyType, SubSupplyType, DocumentType, TransactionType,
                       FromGSTIN, FromTradeName, ToGSTIN, ToTradeName,
                       TotalValue, CGST, SGST, IGST, Cess,
                       TransporterId, TransporterGSTIN, TransporterName, TransportMode,
                       TransDocNo, TransDocDate, VehicleNo, VehicleType, Distance,
                       PartyId, GeneratedDate, ValidUpto,
                       EwbStatus, ErrorCode, ErrorMessage, CancelledDate, CancelReason,
                       IsActive, IsDeleted,
                       CreatedBy, CreatedDate, CreatedByName, CreatedIP,
                       ModifiedBy, ModifiedDate, ModifiedByName, ModifiedIP
                FROM [Finance].[EWaybillHeader]
                WHERE IsDeleted = 0
                {unitFilter}
                AND (@Search IS NULL OR EWBNumber LIKE @Search OR InvoiceNo LIKE @Search)
                ORDER BY Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var parameters = new { Search = search, Offset = offset, PageSize = pageSize, UnitId = unitId };
            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);
            var rows = (await _dbConnection.QueryAsync<EWaybillHeaderDto>(dataSql, parameters)).ToList();

            if (rows.Count > 0)
            {
                var unitIds = rows.Select(r => r.UnitId).Distinct();
                var units = await _unitLookup.GetByIdsAsync(unitIds);
                var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);
                foreach (var item in rows)
                    item.UnitName = unitDict.TryGetValue(item.UnitId, out var uName) ? uName : null;

                var partyIds = rows.Where(r => r.PartyId.HasValue).Select(r => r.PartyId!.Value).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(partyIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);
                foreach (var item in rows.Where(r => r.PartyId.HasValue))
                    item.PartyName = partyDict.TryGetValue(item.PartyId!.Value, out var pName) ? pName : null;
            }

            return (rows, totalCount);
        }

        public async Task<EWaybillHeaderDto?> GetByIdAsync(int id)
        {
            var unitId = _ipAddressService.GetUnitId();
            var unitFilter = unitId.HasValue ? "AND UnitId = @UnitId" : "";

            var sql = $@"
                SELECT Id, EInvoiceHeaderId, UnitId, EWBNumber, InvoiceNo, InvoiceDate, InvoiceValue,
                       SupplyType, SubSupplyType, DocumentType, TransactionType,
                       FromGSTIN, FromTradeName, ToGSTIN, ToTradeName,
                       TotalValue, CGST, SGST, IGST, Cess,
                       TransporterId, TransporterGSTIN, TransporterName, TransportMode,
                       TransDocNo, TransDocDate, VehicleNo, VehicleType, Distance,
                       PartyId, GeneratedDate, ValidUpto,
                       EwbStatus, ErrorCode, ErrorMessage, CancelledDate, CancelReason,
                       IsActive, IsDeleted,
                       CreatedBy, CreatedDate, CreatedByName, CreatedIP,
                       ModifiedBy, ModifiedDate, ModifiedByName, ModifiedIP
                FROM [Finance].[EWaybillHeader]
                WHERE Id = @Id AND IsDeleted = 0 {unitFilter}";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<EWaybillHeaderDto>(sql, new { Id = id, UnitId = unitId });

            if (dto != null)
            {
                var unit = await _unitLookup.GetByIdAsync(dto.UnitId);
                dto.UnitName = unit?.UnitName;

                if (dto.PartyId.HasValue)
                {
                    var party = await _partyLookup.GetByIdAsync(dto.PartyId.Value);
                    dto.PartyName = party?.PartyName;
                }
            }

            return dto;
        }

        public async Task<IReadOnlyList<EWaybillHeaderLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            var unitId = _ipAddressService.GetUnitId();
            var unitFilter = unitId.HasValue ? "AND UnitId = @UnitId" : "";

            var sql = $@"
                SELECT TOP 20 Id, EWBNumber, InvoiceNo, EwbStatus
                FROM [Finance].[EWaybillHeader]
                WHERE IsDeleted = 0 AND IsActive = 1
                {unitFilter}
                AND (EWBNumber LIKE @Term OR InvoiceNo LIKE @Term)
                ORDER BY EWBNumber ASC";

            var result = await _dbConnection.QueryAsync<EWaybillHeaderLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%", UnitId = unitId }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> EWBNumberExistsAsync(string ewbNumber, int? excludeId = null)
        {
            var sql = @"SELECT COUNT(1) FROM [Finance].[EWaybillHeader] WHERE EWBNumber = @EWBNumber AND IsDeleted = 0";
            if (excludeId.HasValue && excludeId.Value > 0)
                sql += " AND Id != @ExcludeId";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { EWBNumber = ewbNumber, ExcludeId = excludeId });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"SELECT COUNT(1) FROM [Finance].[EWaybillHeader] WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }
    }
}
