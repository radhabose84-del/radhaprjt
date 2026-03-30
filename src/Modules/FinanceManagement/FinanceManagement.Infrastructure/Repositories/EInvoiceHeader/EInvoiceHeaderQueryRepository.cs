using System.Data;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Party;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Dto;

namespace FinanceManagement.Infrastructure.Repositories.EInvoiceHeader
{
    public class EInvoiceHeaderQueryRepository : IEInvoiceHeaderQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPartyLookup _partyLookup;
        private readonly IIPAddressService _ipAddressService;

        public EInvoiceHeaderQueryRepository(IDbConnection dbConnection, IPartyLookup partyLookup, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
            _ipAddressService = ipAddressService;
        }

        public async Task<(List<EInvoiceHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;
            var search = string.IsNullOrWhiteSpace(searchTerm) ? null : $"%{searchTerm}%";
            var unitId = _ipAddressService.GetUnitId();
            var unitFilter = unitId.HasValue ? "AND UnitId = @UnitId" : "";

            var countSql = $@"
                SELECT COUNT(*) FROM [Finance].[EInvoiceHeader]
                WHERE IsDeleted = 0
                {unitFilter}
                AND (@Search IS NULL OR InvoiceNo LIKE @Search OR IrnNumber LIKE @Search)";

            var dataSql = $@"
                SELECT Id, UnitId, DocType, SupplyType, InvoiceNo, InvoiceDate, PlaceOfSupply,
                       IrnNumber, AckNo, AckDate, SignInvoice, SignQrCode,
                       IrnStatus, ErrorCode, ErrorMessage,
                       PartyId, GstNo, ReverseCharge,
                       CGST, SGST, IGST, Cess, StateCess, TCS, Discount, OtherCharges, RoundOff, InvoiceAmount,
                       Remarks, StatusId, EWaybillCreated,
                       IsActive, IsDeleted,
                       CreatedBy, CreatedDate, CreatedByName, CreatedIP,
                       ModifiedBy, ModifiedDate, ModifiedByName, ModifiedIP
                FROM [Finance].[EInvoiceHeader]
                WHERE IsDeleted = 0
                {unitFilter}
                AND (@Search IS NULL OR InvoiceNo LIKE @Search OR IrnNumber LIKE @Search)
                ORDER BY Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var parameters = new { Search = search, Offset = offset, PageSize = pageSize, UnitId = unitId };
            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, parameters);
            var rows = (await _dbConnection.QueryAsync<EInvoiceHeaderDto>(dataSql, parameters)).ToList();

            if (rows.Count > 0)
            {
                var partyIds = rows.Select(r => r.PartyId).Distinct();
                var parties = await _partyLookup.GetByIdsAsync(partyIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);
                foreach (var item in rows)
                    item.PartyName = partyDict.TryGetValue(item.PartyId, out var pName) ? pName : null;
            }

            return (rows, totalCount);
        }

        public async Task<EInvoiceHeaderDto?> GetByIdAsync(int id)
        {
            var unitId = _ipAddressService.GetUnitId();
            var unitFilter = unitId.HasValue ? "AND UnitId = @UnitId" : "";

            var sql = $@"
                SELECT Id, UnitId, DocType, SupplyType, InvoiceNo, InvoiceDate, PlaceOfSupply,
                       IrnNumber, AckNo, AckDate, SignInvoice, SignQrCode,
                       IrnStatus, ErrorCode, ErrorMessage,
                       PartyId, GstNo, ReverseCharge,
                       CGST, SGST, IGST, Cess, StateCess, TCS, Discount, OtherCharges, RoundOff, InvoiceAmount,
                       Remarks, StatusId, EWaybillCreated,
                       IsActive, IsDeleted,
                       CreatedBy, CreatedDate, CreatedByName, CreatedIP,
                       ModifiedBy, ModifiedDate, ModifiedByName, ModifiedIP
                FROM [Finance].[EInvoiceHeader]
                WHERE Id = @Id AND IsDeleted = 0 {unitFilter};

                SELECT Id, EInvoiceHeaderId, ItemSno, ItemId, ItemName, HsnNo,
                       NoOfBags, Qty, UnitPrice, Rate, Discount, GrossAmount, TaxableAmount,
                       GstPercentage, CGST, SGST, IGST, CessRate, CessAmount,
                       OtherCharges, TotalAmount, IsService, FreeQty, PackTypeId, UOM
                FROM [Finance].[EInvoiceDetail]
                WHERE EInvoiceHeaderId = @Id;";

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new { Id = id, UnitId = unitId });
            var dto = await multi.ReadFirstOrDefaultAsync<EInvoiceHeaderDto>();

            if (dto != null)
            {
                dto.Details = (await multi.ReadAsync<EInvoiceDetailDto>()).ToList();

                var party = await _partyLookup.GetByIdAsync(dto.PartyId);
                dto.PartyName = party?.PartyName;
            }

            return dto;
        }

        public async Task<IReadOnlyList<EInvoiceHeaderLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            var unitId = _ipAddressService.GetUnitId();
            var unitFilter = unitId.HasValue ? "AND UnitId = @UnitId" : "";

            var sql = $@"
                SELECT TOP 20 Id, InvoiceNo, IrnNumber
                FROM [Finance].[EInvoiceHeader]
                WHERE IsDeleted = 0 AND IsActive = 1
                {unitFilter}
                AND (InvoiceNo LIKE @Term OR IrnNumber LIKE @Term)
                ORDER BY InvoiceNo ASC";

            var result = await _dbConnection.QueryAsync<EInvoiceHeaderLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%", UnitId = unitId }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> IrnNumberExistsAsync(string irnNumber, int? excludeId = null)
        {
            var sql = @"SELECT COUNT(1) FROM [Finance].[EInvoiceHeader] WHERE IrnNumber = @IrnNumber AND IsDeleted = 0";
            if (excludeId.HasValue && excludeId.Value > 0)
                sql += " AND Id != @ExcludeId";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { IrnNumber = irnNumber, ExcludeId = excludeId });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"SELECT COUNT(1) FROM [Finance].[EInvoiceHeader] WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }
    }
}
