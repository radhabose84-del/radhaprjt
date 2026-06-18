using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Application.Common.Interfaces.IGlAccountImport;
using FinanceManagement.Application.GlAccountImport.Dto;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Entities = FinanceManagement.Domain.Entities;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.GlAccountImport
{
    public class GlAccountImportCommandRepository : IGlAccountImportCommandRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IAccountGroupCommandRepository _accountGroupCommandRepository;

        public GlAccountImportCommandRepository(
            ApplicationDbContext dbContext,
            IAccountGroupCommandRepository accountGroupCommandRepository)
        {
            _dbContext = dbContext;
            _accountGroupCommandRepository = accountGroupCommandRepository;
        }

        public async Task<int> CommitAsync(GlAccountImportCommitRequest request, CancellationToken ct)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
                try
                {
                    int invalidRows = request.Errors
                        .Where(e => e.RowNumber > 0)
                        .Select(e => e.RowNumber)
                        .Distinct()
                        .Count();

                    // 1. Log header (saved first so accounts/errors can reference its id).
                    var log = new Entities.GlAccountImportLog
                    {
                        CompanyId = request.CompanyId,
                        FileName = Truncate(request.FileName, 260)!,
                        FileFormat = request.FileFormat,
                        ImportMode = request.ImportMode,
                        TotalRows = request.TotalRows,
                        GroupRows = request.GroupRows,
                        AccountRows = request.AccountRows,
                        ValidRows = Math.Max(0, request.TotalRows - invalidRows),
                        InvalidRows = invalidRows,
                        ImportedGroups = request.Groups.Count,
                        ImportedAccounts = request.Accounts.Count,
                        SkippedRows = invalidRows,
                        Status = request.Status,
                        DurationMs = request.DurationMs,
                        IsActive = Status.Active,
                        IsDeleted = IsDelete.NotDeleted
                    };
                    await _dbContext.GlAccountImportLog.AddAsync(log, ct);
                    await _dbContext.SaveChangesAsync(ct);

                    // 2. New groups, parent-before-child. CreateAsync derives Level and flips the
                    //    parent's IsLeaf — reusing the proven AccountGroup logic (FR-002).
                    var newGroupIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    foreach (var pg in request.Groups)
                    {
                        int? parentId = pg.ExistingParentId;
                        if (parentId == null && !string.IsNullOrWhiteSpace(pg.ParentGroupCode)
                            && newGroupIds.TryGetValue(pg.ParentGroupCode.Trim(), out var pid))
                        {
                            parentId = pid;
                        }

                        var group = new Entities.AccountGroup
                        {
                            CompanyId = request.CompanyId,
                            GroupCode = pg.GroupCode,
                            GroupName = pg.GroupName,
                            ParentAccountGroupId = parentId,
                            AccountTypeId = pg.AccountTypeId,
                            SortOrder = pg.SortOrder,
                            IsActive = Status.Active,        // groups Active so the tree is usable
                            IsDeleted = IsDelete.NotDeleted
                        };

                        var groupId = await _accountGroupCommandRepository.CreateAsync(group);
                        newGroupIds[pg.GroupCode.Trim()] = groupId;
                    }

                    // 3. Accounts — Inactive by default (AC3), stamped with the import-log id.
                    if (request.Accounts.Count > 0)
                    {
                        var accounts = new List<Entities.GlAccountMaster>(request.Accounts.Count);
                        foreach (var pa in request.Accounts)
                        {
                            int groupId = pa.ExistingAccountGroupId
                                ?? newGroupIds[pa.AccountGroupCode.Trim()];

                            accounts.Add(new Entities.GlAccountMaster
                            {
                                CompanyId = request.CompanyId,
                                AccountTypeId = pa.AccountTypeId,
                                AccountGroupId = groupId,
                                AccountCode = pa.AccountCode,
                                AccountName = pa.AccountName,
                                Description = pa.Description,
                                NormalBalanceId = pa.NormalBalanceId,
                                CurrencyTypeId = pa.CurrencyTypeId,
                                SubLedgerTypeId = pa.SubLedgerTypeId,
                                IsCostCentreMandatory = pa.IsCostCentreMandatory,
                                IsTaxRelevant = pa.IsTaxRelevant,
                                IsInterCompany = pa.IsInterCompany,
                                IsReconciliationRequired = pa.IsReconciliationRequired,
                                ImportLogId = log.Id,
                                IsActive = Status.Inactive,
                                IsDeleted = IsDelete.NotDeleted
                            });
                        }
                        await _dbContext.GlAccountMaster.AddRangeAsync(accounts, ct);
                        await _dbContext.SaveChangesAsync(ct);
                    }

                    // 4. Row-error report (retained alongside the log — AC1/AC2).
                    if (request.Errors.Count > 0)
                    {
                        var errors = request.Errors.Select(e => new Entities.GlAccountImportError
                        {
                            ImportLogId = log.Id,
                            RowNumber = e.RowNumber,
                            RecordType = Truncate(e.RecordType, 10),
                            ColumnName = Truncate(e.ColumnName, 100),
                            AttemptedValue = Truncate(e.AttemptedValue, 200),
                            ErrorMessage = Truncate(e.ErrorMessage, 500)!,
                            IsActive = Status.Active,
                            IsDeleted = IsDelete.NotDeleted
                        }).ToList();

                        await _dbContext.GlAccountImportError.AddRangeAsync(errors, ct);
                        await _dbContext.SaveChangesAsync(ct);
                    }

                    await transaction.CommitAsync(ct);
                    return log.Id;
                }
                catch (DbUpdateException dbEx)
                {
                    await transaction.RollbackAsync(ct);
                    throw new Exception($"COA import database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
                }
                catch
                {
                    await transaction.RollbackAsync(ct);
                    throw;
                }
            });
        }

        public async Task<int> ActivateBatchAsync(int importLogId, int companyId, CancellationToken ct)
        {
            var accounts = await _dbContext.GlAccountMaster
                .Where(a => a.ImportLogId == importLogId
                            && a.CompanyId == companyId
                            && a.IsActive == Status.Inactive
                            && a.IsDeleted == IsDelete.NotDeleted)
                .ToListAsync(ct);

            if (accounts.Count == 0)
                return 0;

            foreach (var account in accounts)
                account.IsActive = Status.Active;

            await _dbContext.SaveChangesAsync(ct);
            return accounts.Count;
        }

        private static string? Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
