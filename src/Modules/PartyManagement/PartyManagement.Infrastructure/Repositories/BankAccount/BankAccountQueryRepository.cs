
using System.Data;
using Contracts.Interfaces.Lookups.Users;
using PartyManagement.Application.BankAccount;
using PartyManagement.Application.BankAccount.Query.GetBankAutocomplete;
using PartyManagement.Application.Common.Interfaces.IBankAccount;
using PartyManagement.Domain.Entities;
using Dapper;
using Microsoft.EntityFrameworkCore;
using PartyManagement.Infrastructure.Data;
using static PartyManagement.Domain.Common.BaseEntity;


namespace Infrastructure.Repositories.Party.BankAccounts;

public class BankAccountQueryRepository : IBankAccountQueryRepository
{
    // Owner-type codes (MiscMaster.Code under the BankAccountOwnerType misc type)
    private const string OwnerTypeUnit = "Unit";
    private const string OwnerTypeParty = "Party";

    private readonly ApplicationDbContext _db;
    private readonly IDbConnection _dbConn;
    private readonly IUnitLookup _unitLookup;   // cross-module (UserManagement) — no JOIN to AppData.Unit
    private readonly ICityLookup _cityLookup;   // cross-module (UserManagement)
    private readonly IStateLookup _stateLookup; // cross-module (UserManagement)
    public BankAccountQueryRepository(ApplicationDbContext db, IDbConnection dbConn, IUnitLookup unitLookup, ICityLookup cityLookup, IStateLookup stateLookup)
    {
        _db = db;
        _dbConn = dbConn;
        _unitLookup = unitLookup;
        _cityLookup = cityLookup;
        _stateLookup = stateLookup;
    }

    public async Task<BankAccountDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var bankAccounts = _db.Set<BankAccount>().AsNoTracking();
        var miscMasters  = _db.Set<MiscMaster>().AsNoTracking();
        var bankMasters  = _db.Set<BankMaster>().AsNoTracking();
        var partyMasters = _db.Set<PartyManagement.Domain.Entities.PartyMaster>().AsNoTracking();

        var q =
            from b in bankAccounts
            where b.IsDeleted == IsDelete.NotDeleted && b.Id == id
            // AccountType (left join)
            join mType in miscMasters on b.AccountTypeId equals mType.Id into gjType
            from mType in gjType.DefaultIfEmpty()
            // Branch (left join)
            join mBranch in miscMasters on b.BranchId equals mBranch.Id into gjBranch
            from mBranch in gjBranch.DefaultIfEmpty()
            // Bank (left join)
            join bm in bankMasters on b.BankId equals bm.Id into gjBank
            from bm in gjBank.DefaultIfEmpty()
            // OwnerType (left join, same-module MiscMaster)
            join mOwner in miscMasters on b.OwnerTypeId equals (int?)mOwner.Id into gjOwner
            from mOwner in gjOwner.DefaultIfEmpty()
            // Party owner (left join, same-module PartyMaster) — used only when OwnerType = Party
            join pm in partyMasters on b.OwnerId equals (int?)pm.Id into gjParty
            from pm in gjParty.DefaultIfEmpty()
            select new BankAccountDto
            {
                Id               = b.Id,
                BankId           = b.BankId,
                AccountNumber    = b.AccountNumber,
                AccountHolderName = b.AccountHolderName,
                BranchId         = b.BranchId,
                IFSCCode         = b.IFSCCode,
                SWIFTCode        = b.SWIFTCode,
                AccountTypeId    = b.AccountTypeId,
                IsDefaultAccount = b.IsDefaultAccount,
                IsPrimaryAccount = b.IsPrimaryAccount,
                IBan             = b.IBan,
                OwnerTypeId      = b.OwnerTypeId,
                OwnerId          = b.OwnerId,
                IsActive         =  (int)b.IsActive,
                // Names/Codes from joins
                AccountTypeName  = mType   != null ? mType.Code     : null,   // or mType.Description
                BranchName       = mBranch != null ? mBranch.Code    : null,   // or mBranch.Description
                BankName         = bm      != null ? bm.BankName     : null,
                BankCode         = bm      != null ? bm.BankCode     : null,
                OwnerTypeName    = mOwner  != null ? mOwner.Code     : null,
                // Party name resolved here (same module); Unit name resolved via lookup below.
                OwnerName        = (mOwner != null && mOwner.Code == OwnerTypeParty && pm != null) ? pm.PartyName : null,
                AddressLine1     = b.AddressLine1,
                AddressLine2     = b.AddressLine2,
                CityId           = b.CityId,
                StateId          = b.StateId,
                Pincode          = b.Pincode
            };

        var dto = await q.FirstOrDefaultAsync(ct);

        if (dto != null)
        {
            if (dto.OwnerId is > 0
                && string.Equals(dto.OwnerTypeName, OwnerTypeUnit, StringComparison.OrdinalIgnoreCase))
            {
                var unit = await _unitLookup.GetByIdAsync(dto.OwnerId.Value, ct);
                dto.OwnerName = unit?.UnitName;
            }

            await ResolveCityStateNamesAsync(new[] { dto }, ct);
        }

        return dto;
    }

   public async Task<(IReadOnlyList<BankAccountDto> Items, int Total)> GetAllAsync(
    int pageNumber, int pageSize, string? search, int? bankId, CancellationToken ct)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize   = pageSize   <= 0 ? 20 : pageSize;

        var bankAccounts = _db.Set<BankAccount>().AsNoTracking();
        var miscMasters  = _db.Set<MiscMaster>().AsNoTracking();
        var bankMasters  = _db.Set<BankMaster>().AsNoTracking();
        var partyMasters = _db.Set<PartyManagement.Domain.Entities.PartyMaster>().AsNoTracking();

        var q =
            from b in bankAccounts
            where b.IsDeleted == IsDelete.NotDeleted
            join mType in miscMasters on b.AccountTypeId equals mType.Id into gjType
            from mType in gjType.DefaultIfEmpty()
            join mBranch in miscMasters on b.BranchId equals mBranch.Id into gjBranch
            from mBranch in gjBranch.DefaultIfEmpty()
            join bm in bankMasters on b.BankId equals bm.Id into gjBank
            from bm in gjBank.DefaultIfEmpty()
            join mOwner in miscMasters on b.OwnerTypeId equals (int?)mOwner.Id into gjOwner
            from mOwner in gjOwner.DefaultIfEmpty()
            join pm in partyMasters on b.OwnerId equals (int?)pm.Id into gjParty
            from pm in gjParty.DefaultIfEmpty()
            select new { b, mType, mBranch, bm, mOwner, pm };

        if (bankId.HasValue)
            q = q.Where(x => x.b.BankId == bankId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var like = $"%{search.Trim()}%";
            q = q.Where(x =>
                EF.Functions.Like(x.b.AccountNumber!, like) ||
                (x.b.IFSCCode  != null && EF.Functions.Like(x.b.IFSCCode!, like)) ||
                (x.b.SWIFTCode != null && EF.Functions.Like(x.b.SWIFTCode!, like)) ||
                (x.mType       != null && EF.Functions.Like(x.mType.Code!, like)) ||
                (x.mBranch     != null && EF.Functions.Like(x.mBranch.Code!, like)) ||
                (x.bm          != null && (
                    EF.Functions.Like(x.bm.BankName!, like) ||
                    EF.Functions.Like(x.bm.BankCode!, like)
                )));
        }

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(x => x.b.IsPrimaryAccount)
            .ThenByDescending(x => x.b.IsDefaultAccount)
            .ThenBy(x => x.b.AccountNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new BankAccountDto
            {
                Id               = x.b.Id,
                BankId           = x.b.BankId,
                AccountNumber    = x.b.AccountNumber,
                AccountHolderName    = x.b.AccountHolderName,
                BranchId         = x.b.BranchId,
                IFSCCode         = x.b.IFSCCode,
                SWIFTCode        = x.b.SWIFTCode,
                AccountTypeId    = x.b.AccountTypeId,
                IsDefaultAccount = x.b.IsDefaultAccount,
                IsPrimaryAccount = x.b.IsPrimaryAccount,
                IBan             = x.b.IBan,
                OwnerTypeId      = x.b.OwnerTypeId,
                OwnerId          = x.b.OwnerId,
                IsActive         =  (int)x.b.IsActive,
                AccountTypeName  = x.mType   != null ? x.mType.Code   : null,
                BranchName       = x.mBranch != null ? x.mBranch.Code : null,
                BankName         = x.bm      != null ? x.bm.BankName  : null,
                BankCode         = x.bm      != null ? x.bm.BankCode  : null,
                OwnerTypeName    = x.mOwner  != null ? x.mOwner.Code  : null,
                OwnerName        = (x.mOwner != null && x.mOwner.Code == OwnerTypeParty && x.pm != null) ? x.pm.PartyName : null,
                AddressLine1     = x.b.AddressLine1,
                AddressLine2     = x.b.AddressLine2,
                CityId           = x.b.CityId,
                StateId          = x.b.StateId,
                Pincode          = x.b.Pincode
            })
            .ToListAsync(ct);

        await ResolveUnitOwnerNamesAsync(items, ct);
        await ResolveCityStateNamesAsync(items, ct);

        return (items, total);
    }

    public async Task<IReadOnlyList<BankLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
    {
         term ??= string.Empty;
        const string sql = @"
            SELECT
                BankAccount.Id,
                BankAccount.AccountNumber,
                BankAccount.AccountHolderName,
                BankMaster.BankName,
                BankAccount.IFSCCode,
                BankAccount.SWIFTCode,
                BankAccount.OwnerTypeId,
                ot.Code AS OwnerTypeName,
                BankAccount.OwnerId,
                CASE WHEN ot.Code = @PartyCode THEN pm.PartyName ELSE NULL END AS OwnerName,
                BankAccount.AddressLine1,
                BankAccount.AddressLine2,
                BankAccount.CityId,
                BankAccount.StateId,
                BankAccount.Pincode
            FROM Party.BankAccount WITH (NOLOCK)
            JOIN Party.BankMaster WITH (NOLOCK) on BankAccount.BankId = BankMaster.Id
            LEFT JOIN Party.MiscMaster ot WITH (NOLOCK) on BankAccount.OwnerTypeId = ot.Id AND ot.IsDeleted = 0
            LEFT JOIN Party.PartyMaster pm WITH (NOLOCK) on BankAccount.OwnerId = pm.Id AND pm.IsDeleted = 0
            WHERE BankAccount.IsActive = 1
            AND BankAccount.IsDeleted = 0
            AND (
                    @term = '' OR
                    BankAccount.AccountNumber LIKE @like
                    OR BankAccount.AccountHolderName LIKE @like
                    OR BankAccount.IFSCCode LIKE @like
                    OR BankAccount.SWIFTCode LIKE @like
                )
            ORDER BY BankAccount.Id;";

             var param = new
        {
            term,
            like = "%" + term + "%",
            PartyCode = OwnerTypeParty
        };

        var cmd  = new CommandDefinition(sql, param, cancellationToken: ct);
        var rows = (await _dbConn.QueryAsync<BankLookupDto>(cmd)).AsList();

        await ResolveUnitOwnerNamesAsync(rows, ct);
        await ResolveCityStateNamesAsync(rows, ct);

        return rows;
    }

    // Resolves OwnerName for Unit-type rows via the cross-module IUnitLookup (batched).
    // Party-type rows already have OwnerName populated from the same-module PartyMaster JOIN.
    private async Task ResolveUnitOwnerNamesAsync(IReadOnlyList<BankAccountDto> rows, CancellationToken ct)
    {
        var unitIds = rows
            .Where(r => r.OwnerId is > 0 && string.Equals(r.OwnerTypeName, OwnerTypeUnit, StringComparison.OrdinalIgnoreCase))
            .Select(r => r.OwnerId!.Value)
            .Distinct()
            .ToList();

        if (unitIds.Count == 0) return;

        var units = await _unitLookup.GetByIdsAsync(unitIds, ct);
        var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

        foreach (var r in rows)
        {
            if (r.OwnerId is > 0
                && string.Equals(r.OwnerTypeName, OwnerTypeUnit, StringComparison.OrdinalIgnoreCase)
                && unitDict.TryGetValue(r.OwnerId.Value, out var name))
            {
                r.OwnerName = name;
            }
        }
    }

    private async Task ResolveUnitOwnerNamesAsync(IReadOnlyList<BankLookupDto> rows, CancellationToken ct)
    {
        var unitIds = rows
            .Where(r => r.OwnerId is > 0 && string.Equals(r.OwnerTypeName, OwnerTypeUnit, StringComparison.OrdinalIgnoreCase))
            .Select(r => r.OwnerId!.Value)
            .Distinct()
            .ToList();

        if (unitIds.Count == 0) return;

        var units = await _unitLookup.GetByIdsAsync(unitIds, ct);
        var unitDict = units.ToDictionary(u => u.UnitId, u => u.UnitName);

        foreach (var r in rows)
        {
            if (r.OwnerId is > 0
                && string.Equals(r.OwnerTypeName, OwnerTypeUnit, StringComparison.OrdinalIgnoreCase)
                && unitDict.TryGetValue(r.OwnerId.Value, out var name))
            {
                r.OwnerName = name;
            }
        }
    }

    // Resolves CityName / StateName from the cross-module City/State lookups (batched).
    private async Task ResolveCityStateNamesAsync(IReadOnlyList<BankAccountDto> rows, CancellationToken ct)
    {
        var cityIds  = rows.Where(r => r.CityId  is > 0).Select(r => r.CityId!.Value).Distinct().ToList();
        var stateIds = rows.Where(r => r.StateId is > 0).Select(r => r.StateId!.Value).Distinct().ToList();

        var cityDict = cityIds.Count == 0
            ? new Dictionary<int, string?>()
            : (await _cityLookup.GetByIdsAsync(cityIds, ct)).ToDictionary(c => c.CityId, c => (string?)c.CityName);
        var stateDict = stateIds.Count == 0
            ? new Dictionary<int, string?>()
            : (await _stateLookup.GetByIdsAsync(stateIds, ct)).ToDictionary(s => s.StateId, s => (string?)s.StateName);

        foreach (var r in rows)
        {
            if (r.CityId is > 0 && cityDict.TryGetValue(r.CityId.Value, out var cn)) r.CityName = cn;
            if (r.StateId is > 0 && stateDict.TryGetValue(r.StateId.Value, out var sn)) r.StateName = sn;
        }
    }

    private async Task ResolveCityStateNamesAsync(IReadOnlyList<BankLookupDto> rows, CancellationToken ct)
    {
        var cityIds  = rows.Where(r => r.CityId  is > 0).Select(r => r.CityId!.Value).Distinct().ToList();
        var stateIds = rows.Where(r => r.StateId is > 0).Select(r => r.StateId!.Value).Distinct().ToList();

        var cityDict = cityIds.Count == 0
            ? new Dictionary<int, string?>()
            : (await _cityLookup.GetByIdsAsync(cityIds, ct)).ToDictionary(c => c.CityId, c => (string?)c.CityName);
        var stateDict = stateIds.Count == 0
            ? new Dictionary<int, string?>()
            : (await _stateLookup.GetByIdsAsync(stateIds, ct)).ToDictionary(s => s.StateId, s => (string?)s.StateName);

        foreach (var r in rows)
        {
            if (r.CityId is > 0 && cityDict.TryGetValue(r.CityId.Value, out var cn)) r.CityName = cn;
            if (r.StateId is > 0 && stateDict.TryGetValue(r.StateId.Value, out var sn)) r.StateName = sn;
        }
    }
}
