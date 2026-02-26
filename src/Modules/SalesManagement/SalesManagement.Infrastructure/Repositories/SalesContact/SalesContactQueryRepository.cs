using System.Data;
using Contracts.Interfaces.Lookups.Party;
using Dapper;
using SalesManagement.Application.Common.Interfaces.ISalesContact;
using SalesManagement.Application.SalesContact.Dto;

namespace SalesManagement.Infrastructure.Repositories.SalesContact
{
    public class SalesContactQueryRepository : ISalesContactQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IPartyLookup _partyLookup;

        public SalesContactQueryRepository(
            IDbConnection dbConnection,
            IPartyLookup partyLookup)
        {
            _dbConnection = dbConnection;
            _partyLookup = partyLookup;
        }

        public async Task<(List<SalesContactDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var whereClause = "sc.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND sc.ContactName LIKE @Search";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Sales.SalesContact sc
                LEFT JOIN Sales.MiscMaster mm ON sc.ContactTypeId = mm.Id AND mm.IsDeleted = 0
                WHERE {whereClause};

                SELECT sc.Id, sc.ContactName, sc.MobileNumber, sc.ContactTypeId,
                    mm.Description AS ContactTypeName,
                    sc.PartyId, sc.Email, sc.Remarks,
                    sc.IsActive, sc.IsDeleted,
                    sc.CreatedBy, sc.CreatedDate, sc.CreatedByName, sc.CreatedIP,
                    sc.ModifiedBy, sc.ModifiedDate, sc.ModifiedByName, sc.ModifiedIP
                FROM Sales.SalesContact sc
                LEFT JOIN Sales.MiscMaster mm ON sc.ContactTypeId = mm.Id AND mm.IsDeleted = 0
                WHERE {whereClause}
                ORDER BY sc.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<SalesContactDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            if (list.Count > 0)
            {
                var partyIds = list
                    .Where(x => x.PartyId.HasValue)
                    .Select(x => x.PartyId!.Value)
                    .Distinct();

                var parties = await _partyLookup.GetByIdsAsync(partyIds);
                var partyDict = parties.ToDictionary(p => p.Id, p => p.PartyName);

                foreach (var item in list.Where(x => x.PartyId.HasValue))
                    item.PartyName = partyDict.TryGetValue(item.PartyId!.Value, out var name) ? name : null;
            }

            return (list, totalCount);
        }

        public async Task<SalesContactDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT sc.Id, sc.ContactName, sc.MobileNumber, sc.ContactTypeId,
                    mm.Description AS ContactTypeName,
                    sc.PartyId, sc.Email, sc.Remarks,
                    sc.IsActive, sc.IsDeleted,
                    sc.CreatedBy, sc.CreatedDate, sc.CreatedByName, sc.CreatedIP,
                    sc.ModifiedBy, sc.ModifiedDate, sc.ModifiedByName, sc.ModifiedIP
                FROM Sales.SalesContact sc
                LEFT JOIN Sales.MiscMaster mm ON sc.ContactTypeId = mm.Id AND mm.IsDeleted = 0
                WHERE sc.Id = @Id AND sc.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<SalesContactDto>(sql, new { Id = id });

            if (dto != null && dto.PartyId.HasValue)
            {
                var party = await _partyLookup.GetByIdAsync(dto.PartyId.Value);
                dto.PartyName = party?.PartyName;
            }

            return dto;
        }

        public async Task<IReadOnlyList<SalesContactLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 20 sc.Id, sc.ContactName, sc.MobileNumber,
                    mm.Description AS ContactTypeName
                FROM Sales.SalesContact sc
                LEFT JOIN Sales.MiscMaster mm ON sc.ContactTypeId = mm.Id AND mm.IsDeleted = 0
                WHERE sc.IsDeleted = 0 AND sc.IsActive = 1
                AND sc.ContactName LIKE @Term
                ORDER BY sc.ContactName ASC";

            var rows = (await _dbConnection.QueryAsync<SalesContactLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%" }, cancellationToken: ct))).ToList();

            return rows;
        }

        public async Task<bool> MobileAlreadyExistsAsync(string mobileNumber, int? excludeId = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesContact
                WHERE MobileNumber = @MobileNumber
                AND IsDeleted = 0";

            if (excludeId.HasValue && excludeId.Value > 0)
                sql += " AND Id != @ExcludeId";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql,
                new { MobileNumber = mobileNumber.Trim(), ExcludeId = excludeId });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.SalesContact
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> ContactTypeExistsAsync(int contactTypeId)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Sales.MiscMaster
                WHERE Id = @ContactTypeId AND IsActive = 1 AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { ContactTypeId = contactTypeId });
            return count > 0;
        }
    }
}
