using System.Data;
using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Dapper;

namespace PartyManagement.Infrastructure.Repositories.Lookups
{
    internal sealed class BankAccountLookupRepository : IBankAccountLookup
    {
        private readonly IDbConnection _dbConnection;
        private readonly ICityLookup _cityLookup;   // cross-module (UserManagement)
        private readonly IStateLookup _stateLookup; // cross-module (UserManagement)

        public BankAccountLookupRepository(IDbConnection dbConnection, ICityLookup cityLookup, IStateLookup stateLookup)
        {
            _dbConnection = dbConnection;
            _cityLookup = cityLookup;
            _stateLookup = stateLookup;
        }

        public async Task<bool> ExistsForOwnerTypeAsync(int bankAccountId, string ownerTypeCode, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1
                    FROM Party.BankAccount ba
                    INNER JOIN Party.MiscMaster mm ON ba.OwnerTypeId = mm.Id AND mm.IsDeleted = 0
                    WHERE ba.Id = @Id
                      AND ba.IsDeleted = 0
                      AND ba.IsActive = 1
                      AND mm.Code = @OwnerTypeCode
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(
                new CommandDefinition(sql, new { Id = bankAccountId, OwnerTypeCode = ownerTypeCode }, cancellationToken: ct));
        }

        public async Task<BankAccountLookupDto?> GetByIdAsync(int bankAccountId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT
                    ba.Id,
                    ba.AccountNumber,
                    ba.AccountHolderName,
                    bm.BankName,
                    ba.IFSCCode,
                    ba.SWIFTCode,
                    ba.IBan,
                    ba.AccountTypeId,
                    at.Code AS AccountTypeName,
                    ba.BranchId,
                    br.Code AS BranchName,
                    ba.IsDefaultAccount,
                    ba.IsPrimaryAccount,
                    ba.AddressLine1,
                    ba.AddressLine2,
                    ba.CityId,
                    ba.StateId,
                    ba.Pincode
                FROM Party.BankAccount ba
                LEFT JOIN Party.BankMaster bm ON ba.BankId = bm.Id AND bm.IsDeleted = 0
                LEFT JOIN Party.MiscMaster at ON ba.AccountTypeId = at.Id AND at.IsDeleted = 0
                LEFT JOIN Party.MiscMaster br ON ba.BranchId = br.Id AND br.IsDeleted = 0
                WHERE ba.Id = @Id AND ba.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<BankAccountLookupDto>(
                new CommandDefinition(sql, new { Id = bankAccountId }, cancellationToken: ct));

            if (dto != null)
            {
                // City/State are cross-module (UserManagement) — resolve names via lookups (no JOIN).
                if (dto.CityId is > 0)
                    dto.CityName = (await _cityLookup.GetByIdAsync(dto.CityId.Value, ct))?.CityName;
                if (dto.StateId is > 0)
                    dto.StateName = (await _stateLookup.GetByIdAsync(dto.StateId.Value, ct))?.StateName;
            }

            return dto;
        }

        public async Task<IReadOnlyList<BankAccountLookupDto>> GetByOwnerTypeAsync(string ownerTypeCode, string? term, CancellationToken ct = default)
        {
            term ??= string.Empty;
            const string sql = @"
                SELECT ba.Id, ba.AccountNumber, ba.AccountHolderName, ba.IFSCCode, bm.BankName
                FROM Party.BankAccount ba
                INNER JOIN Party.MiscMaster mm ON ba.OwnerTypeId = mm.Id AND mm.IsDeleted = 0
                LEFT JOIN Party.BankMaster bm ON ba.BankId = bm.Id AND bm.IsDeleted = 0
                WHERE ba.IsActive = 1
                  AND ba.IsDeleted = 0
                  AND mm.Code = @OwnerTypeCode
                  AND (@term = '' OR ba.AccountNumber LIKE @like OR ba.AccountHolderName LIKE @like)
                ORDER BY ba.AccountNumber";

            var rows = await _dbConnection.QueryAsync<BankAccountLookupDto>(
                new CommandDefinition(sql, new { OwnerTypeCode = ownerTypeCode, term, like = "%" + term + "%" }, cancellationToken: ct));
            return rows.AsList();
        }
    }
}
