using BudgetManagement.Infrastructure.Data;
using Contracts.Interfaces.Lookups.Users;
using BudgetManagement.Application.BudgetRequest.Commands.Create;
using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace BudgetManagement.Infrastructure.Repositories.BudgetRequest;

public class BudgetRequestCommandRepository : IBudgetRequestCommandRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
    private readonly IUnitLookup _unitLookup;
    private readonly IIPAddressService _ipAddressService;

    public BudgetRequestCommandRepository(ApplicationDbContext db, IMiscMasterQueryRepository miscMasterQueryRepository,
     IUnitLookup unitLookup, IIPAddressService ipAddressService)
    {
        _db = db;
        _miscMasterQueryRepository = miscMasterQueryRepository;
        _unitLookup = unitLookup;
        _ipAddressService = ipAddressService;
    }

    public async Task<BudgetManagement.Domain.Entities.BudgetRequest> AddAsync(BudgetManagement.Domain.Entities.BudgetRequest entity, CancellationToken ct = default)
    {
        var pending = await _miscMasterQueryRepository.GetMiscMasterByName(
            MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Pending);
        entity.StatusId = pending!.Id;
        await ApplyRequestMonthPeriodAsync(entity, ct);

        _db.BudgetRequests.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(BudgetManagement.Domain.Entities.BudgetRequest entity, CancellationToken ct = default)
    {
        await ApplyRequestMonthPeriodAsync(entity, ct);
        _db.BudgetRequests.Update(entity);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.BudgetRequests
                              .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null) return false;

        entity.IsDeleted = BaseEntity.IsDelete.Deleted;
        await _db.SaveChangesAsync(ct);
        return true;
    }
    public Task<BudgetManagement.Domain.Entities.BudgetRequest?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return _db.BudgetRequests.FirstOrDefaultAsync(x => x.Id == id, ct);
    }
    public async Task<string> GenerateCodeAsync(
           int unitId,
           DateOnly requestDate,
           CancellationToken ct = default)
    {
        var unit = await _unitLookup.GetByIdAsync(unitId, ct);
        if (unit == null)
            throw new InvalidOperationException($"UnitId {unitId} does not exist. Failed to generate Budget Request code.");

        var unitName = string.IsNullOrWhiteSpace(unit.ShortName)
            ? unit.UnitName
            : unit.ShortName;

        if (string.IsNullOrWhiteSpace(unitName))
            throw new InvalidOperationException($"UnitId {unitId} does not have a usable name. Failed to generate Budget Request code.");

        // ----------------- 2. Build 3-char codes -----------------
        string unit3 = BuildCode3(unitName);

        // ----------------- 3. FY range (April–March) -----------------
        var (fyStart, _) = GetFyRange(requestDate, 4);   // FY starts in April
        var fyYear = fyStart.Year;                       // e.g. 2025

        // Pattern: UNIT3-YYYY-SS
        var prefix = $"{unit3}-{fyYear}-";

        // ----------------- 4. Get last used code for this FY -----------------
        // Using RequestCode prefix is enough to reset per FY.
        var lastCode = await _db.BudgetRequests
            .AsNoTracking()
            .Where(b => b.RequestCode != null && b.RequestCode.StartsWith(prefix))
            .OrderByDescending(b => b.RequestCode)
            .Select(b => b.RequestCode!)
            .FirstOrDefaultAsync(ct);

        var nextSerial = 1;

        if (!string.IsNullOrEmpty(lastCode))
        {
            var lastDash = lastCode.LastIndexOf('-');
            if (lastDash >= 0 && int.TryParse(lastCode[(lastDash + 1)..], out var lastSerial))
                nextSerial = lastSerial + 1;
        }

        // 2-digit serial: 01, 02, ..., 99
        var code = $"{prefix}{nextSerial:00}";
        return code;
    }

    private static string BuildCode3(string name)
    {
        // take only alphanumerics, first 3 chars, uppercase.
        var filtered = new string(
            name.Where(char.IsLetterOrDigit)
                .Take(3)
                .ToArray());

        if (string.IsNullOrWhiteSpace(filtered))
            filtered = "XXX";

        return filtered.ToUpperInvariant();
    }

    private static (DateOnly fyStart, DateOnly fyEndExclusive) GetFyRange(DateOnly date, int fyStartMonth)
    {
        var year = date.Month >= fyStartMonth ? date.Year : date.Year - 1;
        var start = new DateOnly(year, fyStartMonth, 1);
        var endEx = start.AddYears(1);
        return (start, endEx);
    }
    public async Task<bool> RemoveImageReferenceAsync(string imageName)
    {
        var entity = await _db.BudgetRequests
                         .FirstOrDefaultAsync(x => x.ImagePath == imageName);

        if (entity == null)
            return false;

        entity.ImagePath = null;
        await _db.SaveChangesAsync();
        return true;
    }
    public async Task<bool> UpdateImageAsync(int requestId, string imageName, CancellationToken ct = default)
    {
        var entity = await _db.BudgetRequests.FindAsync(new object[] { requestId }, ct);
        if (entity == null)
            return false;

        entity.ImagePath = imageName;
        await _db.SaveChangesAsync(ct);
        return true;
    }
    private async Task ApplyRequestMonthPeriodAsync(BudgetManagement.Domain.Entities.BudgetRequest entity, CancellationToken ct)
    {
        // 1. No RequestMonth selected → do nothing
        if (!entity.RequestMonthId.HasValue || entity.RequestMonthId.Value == 0)
            return;

        if (entity.FromDate.HasValue && entity.ToDate.HasValue)
            return;

        var monthMaster = await _miscMasterQueryRepository.GetByIdAsync(entity.RequestMonthId.Value);

        if (monthMaster == null)
            return;

        if (!int.TryParse(monthMaster.Description, out var monthNumber) ||
            monthNumber < 1 || monthNumber > 12)
        {
            return;
        }
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var year = today.Year;

        var fromDate = new DateOnly(year, monthNumber, 1);
        var toDate = fromDate.AddMonths(1).AddDays(-1);

        entity.FromDate = fromDate;
        entity.ToDate = toDate;
    }
    public async Task<bool> UpdateRequestApproveAsync(int requestHeaderId, int statusId, CancellationToken ct = default)
    {
        var approved = await _miscMasterQueryRepository
            .GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Approved);
        var rejected = await _miscMasterQueryRepository
            .GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, MiscEnumEntity.Rejected);

        var qch = await _db.BudgetRequests
            .FirstOrDefaultAsync(h => h.Id == requestHeaderId, ct);
        if (qch is null) return false;
        qch.StatusId = statusId;
        var saveResult = await _db.SaveChangesAsync(ct) > 0;

        if (!saveResult)
            return false;
        return true;
    }
    private async Task<int> GetAllocationTypeId(string miscTypeName)
    {
        var allocationType = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.ApprovalStatus, miscTypeName);
        return allocationType?.Id ?? 0;
    }
    public async Task<bool> RollbackStatusAsync(int id, CancellationToken ct = default)
    {
        // 1. Load entity
        var entity = await _db.BudgetRequests
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == BaseEntity.IsDelete.NotDeleted,
             ct);

        if (entity is null)
            return false;

        // 2. Get “Pending” status from MiscMaster
        var pendingStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
            MiscEnumEntity.ApprovalStatus,
            MiscEnumEntity.Pending);

        if (pendingStatus == null)
            throw new InvalidOperationException("Pending status not configured in MiscMaster.");

        entity.StatusId = pendingStatus.Id;
        return await _db.SaveChangesAsync(ct) > 0;
    }
    public async Task<BudgetRequestWorkFlowDto> GetByIdBudgetRequestWorkFlowAsync(int id)
    {
        var entity = await _db.BudgetRequests
        .Where(x => x.Id == id)
        .Select(x => new BudgetRequestWorkFlowDto
        {
            Id = x.Id,
            RequestCode = x.RequestCode,
            StatusId = x.StatusId,
            UnitId = _ipAddressService.GetUnitId()
        })
        .FirstOrDefaultAsync();
        return entity!;
    }
    public async Task<bool> RollbackRequestStatusAsync(int id, CancellationToken ct = default)
    {
        var existingRequest = await _db.BudgetRequests
            .FirstOrDefaultAsync(p => p.Id == id
                                && p.IsDeleted == BaseEntity.IsDelete.NotDeleted, ct);

        if (existingRequest == null)
            return false;

        var statusMisc = await _miscMasterQueryRepository
            .GetMiscMasterByName(MiscEnumEntity.Status, MiscEnumEntity.Pending);

        existingRequest.StatusId = statusMisc!.Id;
        _db.BudgetRequests.Update(existingRequest);
        return await _db.SaveChangesAsync(ct) > 0;
    }
    public async Task<bool> ExistsOpexAsync(int unitId, int financialYearId, int requestTypeId, int budgetGroupId,  int? requestById, CancellationToken ct)
    {
        return await _db.Set<BudgetManagement.Domain.Entities.BudgetRequest>()
            .AsNoTracking()
            .AnyAsync(x =>
                x.UnitId == unitId &&
                x.FinancialYearId == financialYearId &&
                x.RequestTypeId == requestTypeId &&
                x.ProjectId == null &&
                x.BudgetGroupId == budgetGroupId &&
               // x.FromDate == fromDate &&
              //  x.ToDate == toDate &&
                (requestById == null || x.RequestById == requestById),
                ct);
    }

    public async Task<bool> ExistsCapexAsync(
        int unitId, int financialYearId, int requestTypeId, int projectId,int wbsId,  int? requestById, CancellationToken ct)
    {
        return await _db.Set<BudgetManagement.Domain.Entities.BudgetRequest>()
            .AsNoTracking()
            .AnyAsync(x =>
                x.UnitId == unitId &&
                x.FinancialYearId == financialYearId &&
                x.RequestTypeId == requestTypeId &&
                x.ProjectId == projectId && x.WBSId==wbsId &&
                x.BudgetGroupId == null &&
                //x.FromDate == fromDate &&
                //x.ToDate == toDate &&
                (requestById == null || x.RequestById == requestById),
                ct);
    }
      public async Task<bool> ExistsOpexForUpdateAsync(
        int excludeId,
        int unitId,
        int requestTypeId,
        int budgetGroupId,
        DateOnly fromDate,
        DateOnly toDate,
        int? requestById,
        CancellationToken ct)
    {
        return await _db.Set<BudgetManagement.Domain.Entities.BudgetRequest>()
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id != excludeId &&
                x.UnitId == unitId &&
                x.RequestTypeId == requestTypeId &&
                x.ProjectId == null &&                 // ✅ OPEX rule
                x.BudgetGroupId == budgetGroupId &&
                x.FromDate == fromDate &&
                x.ToDate == toDate &&
                (requestById == null || x.RequestById == requestById),
                ct);
    }

    public async Task<bool> ExistsCapexForUpdateAsync(
        int excludeId,
        int unitId,
        int requestTypeId,
        int projectId,int wbsId,
        DateOnly fromDate,
        DateOnly toDate,
        int? requestById,
        CancellationToken ct)
    {
        return await _db.Set<BudgetManagement.Domain.Entities.BudgetRequest>()
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id != excludeId &&
                x.UnitId == unitId &&
                x.RequestTypeId == requestTypeId &&
                x.ProjectId == projectId && x.WBSId==wbsId &&
                x.BudgetGroupId == null &&  
                x.FromDate == fromDate &&
                x.ToDate == toDate &&
                (requestById == null || x.RequestById == requestById),
                ct);
    }
}
