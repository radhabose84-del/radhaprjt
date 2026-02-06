
using System.Data;
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
    private readonly ApplicationDbContext _db;
    private readonly IDbConnection _dbConn;
    public BankAccountQueryRepository(ApplicationDbContext db, IDbConnection dbConn)
    {
        _db = db;
        _dbConn = dbConn;       
    } 

    public async Task<BankAccountDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var bankAccounts = _db.Set<BankAccount>().AsNoTracking();
        var miscMasters  = _db.Set<MiscMaster>().AsNoTracking();
        var bankMasters  = _db.Set<BankMaster>().AsNoTracking();

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
                IsActive         =  (int)b.IsActive, 
                // Names/Codes from joins
                AccountTypeName  = mType   != null ? mType.Code     : null,   // or mType.Description
                BranchName       = mBranch != null ? mBranch.Code    : null,   // or mBranch.Description
                BankName         = bm      != null ? bm.BankName     : null,
                BankCode         = bm      != null ? bm.BankCode     : null
            };

        return await q.FirstOrDefaultAsync(ct);
    }

   public async Task<(IReadOnlyList<BankAccountDto> Items, int Total)> GetAllAsync(
    int pageNumber, int pageSize, string? search, int? bankId, CancellationToken ct)
    {
        pageNumber = pageNumber <= 0 ? 1 : pageNumber;
        pageSize   = pageSize   <= 0 ? 20 : pageSize;

        var bankAccounts = _db.Set<BankAccount>().AsNoTracking();
        var miscMasters  = _db.Set<MiscMaster>().AsNoTracking();
        var bankMasters  = _db.Set<BankMaster>().AsNoTracking();

        var q =
            from b in bankAccounts
            where b.IsDeleted == IsDelete.NotDeleted
            join mType in miscMasters on b.AccountTypeId equals mType.Id into gjType
            from mType in gjType.DefaultIfEmpty()
            join mBranch in miscMasters on b.BranchId equals mBranch.Id into gjBranch
            from mBranch in gjBranch.DefaultIfEmpty()
            join bm in bankMasters on b.BankId equals bm.Id into gjBank
            from bm in gjBank.DefaultIfEmpty()
            select new { b, mType, mBranch, bm };

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
                IsActive         =  (int)x.b.IsActive, 
                AccountTypeName  = x.mType   != null ? x.mType.Code   : null,
                BranchName       = x.mBranch != null ? x.mBranch.Code : null,
                BankName         = x.bm      != null ? x.bm.BankName  : null,
                BankCode         = x.bm      != null ? x.bm.BankCode  : null
            })
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IReadOnlyList<BankLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
    {        
         term ??= string.Empty;
        const string sql = @"
            SELECT 
                BankAccount.Id,
                AccountNumber,
                IFSCCode,
                SWIFTCode,AccountHolderName
            FROM Party.BankAccount WITH (NOLOCK)
            join Party.BankMaster WITH (NOLOCK) on BankAccount.BankId = BankMaster.Id
            WHERE BankAccount.IsActive = 1
            AND BankAccount.IsDeleted = 0
            AND (
                    @term = '' OR                    
                    AccountNumber LIKE @like
                    OR AccountHolderName LIKE @like
                    OR IFSCCode LIKE @like
                    OR SWIFTCode LIKE @like
                )
            ORDER BY Id;";
                    
             var param = new
        {
            term,
            like = "%" + term + "%"
        };

        var cmd  = new CommandDefinition(sql, param, cancellationToken: ct);
        var rows = await _dbConn.QueryAsync<BankLookupDto>(cmd);
        return rows.AsList();
    }
}