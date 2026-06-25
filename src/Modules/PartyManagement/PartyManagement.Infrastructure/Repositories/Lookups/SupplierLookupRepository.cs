using System.Data;
using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Party;
using Dapper;

namespace PartyManagement.Infrastructure.Repositories.Lookups
{
    /// <summary>
    /// Returns active suppliers from the ERP Party Master. A party is a "Supplier"
    /// when it has a Party.PartyType row whose PartyTypeId → Party.MiscMaster.Code
    /// is 'Supplier' (matched case-insensitively, mirroring GetPartyTypeCodesAsync).
    ///
    /// Results are scoped to the caller's current unit via
    /// Party.PartyUnitCompanyMapping (UnitId = IIPAddressService.GetUnitId()),
    /// consistent with the standard party autocomplete.
    /// </summary>
    internal sealed class SupplierLookupRepository : ISupplierLookup
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;

        public SupplierLookupRepository(IDbConnection dbConnection, IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }

        public async Task<IReadOnlyList<SupplierLookupDto>> SearchSuppliersAsync(
            string? term, CancellationToken ct = default)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            const string sql = @"
                SELECT a.Id,
                       a.PartyCode AS VendorCode,
                       a.PartyName AS VendorName
                FROM Party.PartyMaster a
                INNER JOIN Party.PartyType pt ON pt.PartyId = a.Id
                INNER JOIN Party.MiscMaster mm ON mm.Id = pt.PartyTypeId
                INNER JOIN Party.PartyUnitCompanyMapping uc ON uc.PartyId = a.Id AND uc.UnitId = @UnitId
                WHERE a.IsActive = 1
                  AND a.IsDeleted = 0
                  AND mm.IsActive = 1
                  AND mm.IsDeleted = 0
                  AND UPPER(mm.Code) = 'SUPPLIER'
                  AND (@Term IS NULL OR a.PartyName LIKE @Pattern OR a.PartyCode LIKE @Pattern)
                GROUP BY a.Id, a.PartyCode, a.PartyName
                ORDER BY a.PartyName ASC;";

            var trimmed = string.IsNullOrWhiteSpace(term) ? null : term.Trim();
            var result = await _dbConnection.QueryAsync<SupplierLookupDto>(
                new CommandDefinition(
                    sql,
                    new { Term = trimmed, Pattern = $"%{trimmed}%", UnitId = unitId },
                    cancellationToken: ct));

            return result.ToList();
        }

        public async Task<SupplierLookupDto?> GetActiveSupplierByIdAsync(
            int partyId, CancellationToken ct = default)
        {
            if (partyId <= 0)
                return null;

            var unitId = _ipAddressService.GetUnitId() ?? 0;

            const string sql = @"
                SELECT TOP 1
                       a.Id,
                       a.PartyCode AS VendorCode,
                       a.PartyName AS VendorName
                FROM Party.PartyMaster a
                INNER JOIN Party.PartyType pt ON pt.PartyId = a.Id
                INNER JOIN Party.MiscMaster mm ON mm.Id = pt.PartyTypeId
                INNER JOIN Party.PartyUnitCompanyMapping uc ON uc.PartyId = a.Id AND uc.UnitId = @UnitId
                WHERE a.Id = @PartyId
                  AND a.IsActive = 1
                  AND a.IsDeleted = 0
                  AND mm.IsActive = 1
                  AND mm.IsDeleted = 0
                  AND UPPER(mm.Code) = 'SUPPLIER';";

            return await _dbConnection.QueryFirstOrDefaultAsync<SupplierLookupDto>(
                new CommandDefinition(sql, new { PartyId = partyId, UnitId = unitId }, cancellationToken: ct));
        }

        public async Task<SupplierLookupDto?> GetActiveSupplierOrGinnerByIdAsync(
            int partyId, CancellationToken ct = default)
        {
            if (partyId <= 0)
                return null;

            var unitId = _ipAddressService.GetUnitId() ?? 0;

            // OCR sources cotton from a Supplier OR a Ginner — accept either party type.
            const string sql = @"
                SELECT TOP 1
                       a.Id,
                       a.PartyCode AS VendorCode,
                       a.PartyName AS VendorName
                FROM Party.PartyMaster a
                INNER JOIN Party.PartyType pt ON pt.PartyId = a.Id
                INNER JOIN Party.MiscMaster mm ON mm.Id = pt.PartyTypeId
                INNER JOIN Party.PartyUnitCompanyMapping uc ON uc.PartyId = a.Id AND uc.UnitId = @UnitId
                WHERE a.Id = @PartyId
                  AND a.IsActive = 1
                  AND a.IsDeleted = 0
                  AND mm.IsActive = 1
                  AND mm.IsDeleted = 0
                  AND UPPER(mm.Code) IN ('SUPPLIER', 'GINNER');";

            return await _dbConnection.QueryFirstOrDefaultAsync<SupplierLookupDto>(
                new CommandDefinition(sql, new { PartyId = partyId, UnitId = unitId }, cancellationToken: ct));
        }
    }
}
