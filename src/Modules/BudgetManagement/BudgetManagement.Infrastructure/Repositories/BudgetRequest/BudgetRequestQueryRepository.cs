using System.Data;
using BudgetManagement.Application.BudgetRequest;
using BudgetManagement.Application.BudgetRequest.Queries.GetBudgetRequestPending;
using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Domain.Common;
using Dapper;
using Microsoft.Extensions.Logging;

namespace BudgetManagement.Infrastructure.Repositories.BudgetRequest;

public class BudgetRequestQueryRepository : IBudgetRequestQueryRepository
{
    private readonly IDbConnection _db;
    private readonly ILogger<BudgetRequestQueryRepository> _logger;
    private readonly IIPAddressService _ipAddressService;
    private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;

    public BudgetRequestQueryRepository(
        IDbConnection db,
        ILogger<BudgetRequestQueryRepository> logger,
        IIPAddressService ipAddressService,
        IMiscMasterQueryRepository miscMasterQueryRepository)
    {
        _db = db;
        _logger = logger;
        _ipAddressService = ipAddressService;
        _miscMasterQueryRepository = miscMasterQueryRepository;
    }

    public async Task<(List<BudgetRequestListItemDto> Items, int Total)> GetAllAsync(
    int? statusId, int pageNumber, int pageSize, string? searchTerm, CancellationToken ct = default)
    {
        var skip = (pageNumber - 1) * pageSize;
        var UnitId = _ipAddressService.GetUnitId();
         var listSql = @"
            SELECT  
                br.Id,
                br.RequestCode,
                br.UnitId,
                br.FinancialYearId,
                br.CurrencyId,
                br.RequestTypeId,
                br.RequestMonthId,
                br.RequestById,
                br.FromDate,
                br.ToDate,
                br.BudgetGroupId,
                br.ProjectId,
                br.WBSId,
                br.StatusId,
                br.RequestAmount,
                br.Remarks,
                br.ImagePath,

                br.CreatedBy,
                br.CreatedByName,
                br.CreatedDate,
                br.ModifiedBy,
                br.ModifiedByName,
                br.ModifiedDate,
                br.IsActive,

                mrt.Code AS RequestTypeName,
                ms.Code  AS StatusName,
                m.Code   AS RequestMonthName,
                mm.Code  AS RequestByName,
                G.Name   AS BudgetGroupName
            FROM Budget.BudgetRequest br
            LEFT JOIN Budget.BudgetGroup     G   ON G.Id   = br.BudgetGroupId
            LEFT JOIN Budget.MiscMaster      mrt ON mrt.Id = br.RequestTypeId
            LEFT JOIN Budget.MiscMaster      ms  ON ms.Id  = br.StatusId
            LEFT JOIN Budget.MiscMaster      m   ON m.Id   = br.RequestMonthId
            LEFT JOIN Budget.MiscMaster      mm  ON mm.Id  = br.RequestById
            WHERE br.UnitId   = @UnitId
            AND br.IsDeleted = 0
            AND (@StatusId IS NULL OR br.StatusId = @StatusId)
            AND (
                    @Search IS NULL 
                    OR br.RequestCode   LIKE @Search
                    OR br.Remarks       LIKE @Search
                    OR mrt.Code         LIKE @Search   -- RequestTypeName
                    OR ms.Code          LIKE @Search   -- StatusName
                    OR m.Code           LIKE @Search   -- RequestMonthName
                    OR mm.Code          LIKE @Search   -- RequestByName
                    OR G.Name           LIKE @Search   -- BudgetGroupName
                )
            ORDER BY br.Id DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
        ";

        var countSql = @"
            SELECT COUNT(1)
            FROM Budget.BudgetRequest br
            LEFT JOIN Budget.BudgetGroup     G   ON G.Id   = br.BudgetGroupId
            LEFT JOIN Budget.MiscMaster      mrt ON mrt.Id = br.RequestTypeId
            LEFT JOIN Budget.MiscMaster      ms  ON ms.Id  = br.StatusId
            LEFT JOIN Budget.MiscMaster      m   ON m.Id   = br.RequestMonthId
            LEFT JOIN Budget.MiscMaster      mm  ON mm.Id  = br.RequestById
            WHERE br.UnitId   = @UnitId
            AND br.IsDeleted = 0
            AND (@StatusId IS NULL OR br.StatusId = @StatusId)
            AND (
                    @Search IS NULL 
                    OR br.RequestCode   LIKE @Search
                    OR br.Remarks       LIKE @Search
                    OR mrt.Code         LIKE @Search
                    OR ms.Code          LIKE @Search
                    OR m.Code           LIKE @Search
                    OR mm.Code          LIKE @Search
                    OR G.Name           LIKE @Search
                );
        ";

        var search = string.IsNullOrWhiteSpace(searchTerm)
            ? null
            : $"%{searchTerm.Trim()}%";

        var param = new
        {
            StatusId = statusId,
            Search = search,
            Skip = skip,
            Take = pageSize,
            UnitId
        };

        // 1) list
        var rawList = (await _db.QueryAsync<BudgetRequestListRaw>(listSql, param)).ToList();

        // 2) total
        var total = await _db.ExecuteScalarAsync<int>(countSql, param);

        if (!rawList.Any())
            return (new List<BudgetRequestListItemDto>(), total);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var items = rawList.Select(r =>
        {
            var rawStatus = (r.StatusName ?? string.Empty).Trim();

            var isPending = string.Equals(
                rawStatus,
                MiscEnumEntity.Pending,
                StringComparison.OrdinalIgnoreCase);

            var isApproved = string.Equals(
                rawStatus,
                MiscEnumEntity.Approved,
                StringComparison.OrdinalIgnoreCase);

            int editFlag;
            string? reason;

            if (r.FromDate.HasValue && r.ToDate.HasValue)
            {
                if (today > r.ToDate.Value)
                {
                    editFlag = 2;
                    reason = "Cannot be edited after its month period.";
                }
                else if (today >= r.FromDate.Value && today <= r.ToDate.Value)
                {
                    if (isPending)
                    {
                        editFlag = 0;
                        reason = "Status Pending and within request period.";
                    }
                    else if (isApproved)
                    {
                        editFlag = 1;
                        reason = "Status Approved – edit will be treated as revision.";
                    }
                    else
                    {
                        editFlag = 2;
                        reason = "Edit not allowed in current status.";
                    }
                }
                else
                {
                    if (isPending)
                    {
                        editFlag = 0;
                        reason = "Status Pending – request period not yet started.";
                    }
                    else if (isApproved)
                    {
                        editFlag = 1;
                        reason = "Status Approved – request period not yet started (revision allowed).";
                    }
                    else
                    {
                        editFlag = 2;
                        reason = "Edit not allowed in current status.";
                    }
                }
            }
            else
            {
                if (isPending)
                {
                    editFlag = 0;
                    reason = "Status Pending.";
                }
                else if (isApproved)
                {
                    editFlag = 1;
                    reason = "Status Approved – edit will be treated as revision.";
                }
                else
                {
                    editFlag = 2;
                    reason = "Edit not allowed in current status.";
                }
            }

            return new BudgetRequestListItemDto
            {
                Id = r.Id,
                RequestCode = r.RequestCode,
                UnitId = r.UnitId,
                FinancialYearId = r.FinancialYearId,
                CurrencyId = r.CurrencyId,
                RequestTypeId = r.RequestTypeId,
                RequestMonthId = r.RequestMonthId,
                RequestById = r.RequestById,
                FromDate = r.FromDate,
                ToDate = r.ToDate,
                BudgetGroupId = r.BudgetGroupId,
                ProjectId = r.ProjectId,
                WBSId = r.WBSId,
                StatusId = r.StatusId,
                RequestAmount = r.RequestAmount,
                Remarks = r.Remarks,
                ImagePath = r.ImagePath,

                CreatedBy = r.CreatedBy,
                CreatedByName = r.CreatedByName,
                CreatedDate = r.CreatedDate,
                ModifiedBy = r.ModifiedBy,
                ModifiedByName = r.ModifiedByName,
                ModifiedDate = r.ModifiedDate,
                IsActive = r.IsActive,

                RequestTypeName = r.RequestTypeName,
                StatusName = r.StatusName,
                RequestByName = r.RequestByName,
                RequestMonthName = r.RequestMonthName,
                BudgetGroupName = r.BudgetGroupName ?? string.Empty,
                EditFlag = editFlag,
                EditReason = reason
            };
        }).ToList();

        return (items, total);
    }
    public async Task<BudgetRequestDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var sql = @"
        SELECT TOP 1
            br.Id,
            br.RequestCode,
            br.UnitId,
            br.CurrencyId,
            br.RequestTypeId,
            br.FromDate,
            br.ToDate,
            br.BudgetGroupId,
            br.ProjectId,
            br.StatusId,
            br.RequestAmount,
            br.Remarks,
            br.ImagePath
        FROM Budget.BudgetRequest br
        WHERE br.Id = @Id AND br.IsDeleted = 0;";

        return await _db.QueryFirstOrDefaultAsync<BudgetRequestDto>(sql, new { Id = id });
    }

    public async Task<string?> GetBaseDirectoryAsync(CancellationToken ct = default)
    {
        const string sql = @"
            SELECT TOP (1) [Description]
            FROM Budget.MiscTypeMaster
            WHERE MiscTypeCode = @Code
            AND IsDeleted   = 0
            AND IsActive    = 1;";

        return await _db.QueryFirstOrDefaultAsync<string>(
            sql,
            new { Code = MiscEnumEntity.ImagePath });
    }

    public async Task<(List<GetBudgetRequestPendingDto> Items, int Total)> GetPendingRequestAsync(
        int pageNumber,
        int pageSize,
        string? search,
        CancellationToken ct = default)
    {
        var skip = (pageNumber - 1) * pageSize;
        var like = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%";
        var unitId = _ipAddressService.GetUnitId();

        // ApprovalStatus = Pending
        var pending = await _miscMasterQueryRepository.GetMiscMasterByName(
            MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);

        var listSql = @"
            SELECT  
                br.Id,
                br.RequestCode,
                br.UnitId,
                br.FinancialYearId,
                br.CurrencyId,
                br.RequestTypeId,
                br.RequestMonthId,
                br.RequestById,
                br.FromDate,
                br.ToDate,
                br.BudgetGroupId,
                br.ProjectId,
                br.StatusId,
                br.RequestAmount,
                br.Remarks,
                br.ImagePath,
                br.RevisionNumber,

                br.CreatedBy,
                br.CreatedByName,
                br.CreatedDate,
                br.ModifiedBy,
                br.ModifiedByName,
                br.ModifiedDate,
                br.IsActive,

                mrt.Code AS RequestTypeName,
                ms.Code  AS StatusName,
                mBy.Code AS RequestByName,
                mMonth.Code AS RequestMonthName
            FROM Budget.BudgetRequest br
                LEFT JOIN Budget.MiscMaster mrt    ON mrt.Id    = br.RequestTypeId
                LEFT JOIN Budget.MiscMaster ms     ON ms.Id     = br.StatusId
                LEFT JOIN Budget.MiscMaster mBy    ON mBy.Id    = br.RequestById
                LEFT JOIN Budget.MiscMaster mMonth ON mMonth.Id = br.RequestMonthId
            WHERE br.IsDeleted = 0
              AND br.UnitId = @UnitId
              AND br.StatusId = @PendingStatusId
              AND (@Search IS NULL 
                    OR br.RequestCode LIKE @Search
                    OR br.Remarks    LIKE @Search)
            ORDER BY br.Id DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
        ";

        var countSql = @"
            SELECT COUNT(1)
            FROM Budget.BudgetRequest br
                LEFT JOIN Budget.MiscMaster ms ON ms.Id = br.StatusId
            WHERE br.IsDeleted = 0
              AND br.UnitId = @UnitId
              AND br.StatusId = @PendingStatusId
              AND (@Search IS NULL 
                    OR br.RequestCode LIKE @Search
                    OR br.Remarks    LIKE @Search);
        ";

        var param = new
        {
            UnitId = unitId,
            PendingStatusId = pending.Id,
            Search = like,
            Skip = skip,
            Take = pageSize
        };

        var items = (await _db.QueryAsync<GetBudgetRequestPendingDto>(listSql, param)).ToList();
        var total = await _db.ExecuteScalarAsync<int>(countSql, param);

        return (items, total);
    }

    public async Task<bool> AllocationExistsAsync(int financialYearId, int requestById, int? requestMonthId, int? budgetGroupId, int? projectId, int? wbsId, CancellationToken ct = default)
    {
        var unitId = _ipAddressService.GetUnitId();
        var sql = @"
        SELECT CASE WHEN EXISTS (
            SELECT 1
            FROM Budget.BudgetAllocation
            WHERE UnitId = @UnitId
            AND FinancialYearId = @FinancialYearId
            AND RequestById = @RequestById
            AND (@RequestMonthId IS NULL OR RequestMonthId = @RequestMonthId)
            AND (@BudgetGroupId IS NULL OR BudgetGroupId = @BudgetGroupId)            
            AND (@ProjectId IS NULL OR ProjectId = @ProjectId)
            AND (@WbsId IS NULL OR WbsId = @WbsId)           
        ) THEN 1 ELSE 0 END";

        var result = await _db.ExecuteScalarAsync<int>(sql, new
        {
            UnitId = unitId,
            FinancialYearId = financialYearId,
            RequestById = requestById,
            RequestMonthId = requestMonthId,
            BudgetGroupId = budgetGroupId,            
            ProjectId = projectId,
            WbsId = wbsId
        });

        return result == 1;
    }
}

