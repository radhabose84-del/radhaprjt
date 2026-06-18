using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.Infrastructure.Repositories.ScheduleIII
{
    public class ScheduleIIICommandRepository : IScheduleIIICommandRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;

        public ScheduleIIICommandRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        // ── Master (thin record) ─────────────────────────────────────────────

        public async Task<int> CreateMasterAsync(ScheduleIIIMaster entity)
        {
            // A new master always starts as DRAFT (MiscMaster S3_STATUS = DRAFT) — status is not client-supplied.
            entity.StatusId = await ResolveMiscIdAsync("S3_STATUS", "DRAFT");

            await _applicationDbContext.ScheduleIIIMaster.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateMasterAsync(ScheduleIIIMaster entity)
        {
            var existing = await _applicationDbContext.ScheduleIIIMaster
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // CompanyId / DivisionId / ScheduleIIISectionItemId are immutable (the row identity).
            existing.StatusId = entity.StatusId;
            existing.TextileSplitEnabled = entity.TextileSplitEnabled;
            existing.DisplayOrder = entity.DisplayOrder;
            existing.IsActive = entity.IsActive;

            _applicationDbContext.ScheduleIIIMaster.Update(existing);
            await _applicationDbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteMasterAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.ScheduleIIIMaster
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.ScheduleIIIMaster.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> ReorderMasterAsync(int masterId, int direction, CancellationToken ct)
        {
            var row = await _applicationDbContext.ScheduleIIIMaster
                .FirstOrDefaultAsync(x => x.Id == masterId && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (row == null)
                return false;

            // Siblings = same structure (Company, Division), excluding self, not deleted.
            var siblings = _applicationDbContext.ScheduleIIIMaster
                .Where(x => x.CompanyId == row.CompanyId
                         && x.DivisionId == row.DivisionId
                         && x.Id != row.Id
                         && x.IsDeleted == IsDelete.NotDeleted);

            ScheduleIIIMaster? neighbour = direction == 1
                ? await siblings.Where(x => x.DisplayOrder < row.DisplayOrder)
                                .OrderByDescending(x => x.DisplayOrder).FirstOrDefaultAsync(ct)
                : await siblings.Where(x => x.DisplayOrder > row.DisplayOrder)
                                .OrderBy(x => x.DisplayOrder).FirstOrDefaultAsync(ct);

            if (neighbour == null)
                return false;

            (row.DisplayOrder, neighbour.DisplayOrder) = (neighbour.DisplayOrder, row.DisplayOrder);

            _applicationDbContext.ScheduleIIIMaster.Update(row);
            _applicationDbContext.ScheduleIIIMaster.Update(neighbour);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> LockStructureAsync(int scheduleIIIMasterId)
        {
            var lockedStatusId = await ResolveMiscIdAsync("S3_STATUS", "LOCKED");
            if (lockedStatusId == 0)
                return false;

            var row = await _applicationDbContext.ScheduleIIIMaster
                .FirstOrDefaultAsync(x => x.Id == scheduleIIIMasterId && x.IsDeleted == IsDelete.NotDeleted);

            if (row == null)
                return false;

            // Lock the whole structure — every row sharing (Company, Division).
            var structureRows = await _applicationDbContext.ScheduleIIIMaster
                .Where(x => x.CompanyId == row.CompanyId && x.DivisionId == row.DivisionId && x.IsDeleted == IsDelete.NotDeleted)
                .ToListAsync();

            foreach (var r in structureRows)
            {
                r.StatusId = lockedStatusId;
                _applicationDbContext.ScheduleIIIMaster.Update(r);
            }
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }

        // ── Section (global catalog) ─────────────────────────────────────────

        public async Task<int> CreateSectionAsync(ScheduleIIISection entity)
        {
            await _applicationDbContext.ScheduleIIISection.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateSectionAsync(ScheduleIIISection entity)
        {
            var existing = await _applicationDbContext.ScheduleIIISection
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.SectionName = entity.SectionName;
            existing.StatementTypeId = entity.StatementTypeId;
            existing.NatureId = entity.NatureId;
            existing.IsActive = entity.IsActive;

            _applicationDbContext.ScheduleIIISection.Update(existing);
            await _applicationDbContext.SaveChangesAsync();
            return existing.Id;
        }

        // ── LineItem (global catalog) ────────────────────────────────────────

        public async Task<int> CreateLineItemAsync(ScheduleIIISectionItem entity)
        {
            await _applicationDbContext.ScheduleIIISectionItem.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateLineItemAsync(ScheduleIIISectionItem entity)
        {
            var existing = await _applicationDbContext.ScheduleIIISectionItem
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // LineCode is immutable — only mutable fields are updated.
            existing.SectionId = entity.SectionId;
            existing.LineName = entity.LineName;
            existing.NoteReference = entity.NoteReference;
            existing.IsSplitLine = entity.IsSplitLine;
            existing.IsActive = entity.IsActive;

            _applicationDbContext.ScheduleIIISectionItem.Update(existing);
            await _applicationDbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteLineItemAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.ScheduleIIISectionItem
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.ScheduleIIISectionItem.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }

        // ── Sub-totals (+ formula operands) ──────────────────────────────────

        public async Task<int> CreateSubTotalAsync(ScheduleIIISubTotal subTotal, List<ScheduleIIISubTotalFormula> formulas)
        {
            subTotal.FormulaExpression = await BuildExpressionAsync(formulas);

            await _applicationDbContext.ScheduleIIISubTotal.AddAsync(subTotal);
            await _applicationDbContext.SaveChangesAsync();

            foreach (var f in formulas)
                f.SubTotalId = subTotal.Id;

            await _applicationDbContext.ScheduleIIISubTotalFormula.AddRangeAsync(formulas);
            await _applicationDbContext.SaveChangesAsync();

            return subTotal.Id;
        }

        public async Task<int> UpdateSubTotalAsync(int subTotalId, string? formulaName, bool includeOtherIncome, List<ScheduleIIISubTotalFormula> formulas)
        {
            var existing = await _applicationDbContext.ScheduleIIISubTotal
                .FirstOrDefaultAsync(x => x.Id == subTotalId && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.FormulaName = formulaName;
            existing.IncludeOtherIncome = includeOtherIncome;
            existing.FormulaExpression = await BuildExpressionAsync(formulas);

            // Replace the operand set: PHYSICALLY delete the old rows, then insert the new ones.
            // ScheduleIIISubTotalFormula is IActivityTracked, so the SaveChanges interceptor records each
            // hard delete as a "Delete" entry in Finance.ActivityLog (with the old operand values).
            var oldFormulas = await _applicationDbContext.ScheduleIIISubTotalFormula
                .Where(f => f.SubTotalId == subTotalId)
                .ToListAsync();

            _applicationDbContext.ScheduleIIISubTotalFormula.RemoveRange(oldFormulas);

            foreach (var f in formulas)
                f.SubTotalId = subTotalId;

            await _applicationDbContext.ScheduleIIISubTotalFormula.AddRangeAsync(formulas);
            _applicationDbContext.ScheduleIIISubTotal.Update(existing);
            await _applicationDbContext.SaveChangesAsync();

            return existing.Id;
        }

        // Rebuilds the cached display string (e.g. "Gross Profit + Other Income - Operating Expenses").
        private async Task<string> BuildExpressionAsync(List<ScheduleIIISubTotalFormula> formulas)
        {
            if (formulas == null || formulas.Count == 0)
                return string.Empty;

            var ordered = formulas.OrderBy(f => f.DisplayOrder).ToList();

            var operatorIds = ordered.Select(f => f.OperatorId).Distinct().ToList();
            var operatorCodes = await _applicationDbContext.MiscMaster
                .Where(m => operatorIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => m.Code);

            // Operands reference a line item directly (SectionItemId).
            var sectionItemIds = ordered.Where(f => f.SectionItemId.HasValue)
                .Select(f => f.SectionItemId!.Value).Distinct().ToList();
            var lineNames = await _applicationDbContext.ScheduleIIISectionItem
                .Where(l => sectionItemIds.Contains(l.Id))
                .ToDictionaryAsync(l => l.Id, l => l.LineName);

            var parts = new List<string>();
            foreach (var f in ordered)
            {
                var opCode = operatorCodes.TryGetValue(f.OperatorId, out var oc) ? oc : string.Empty;
                var symbol = string.Equals(opCode, "MINUS", StringComparison.OrdinalIgnoreCase) ? "-" : "+";

                var name = f.SectionItemId.HasValue && lineNames.TryGetValue(f.SectionItemId.Value, out var ln) ? ln : null;

                parts.Add($"{symbol} {name}");
            }

            var expression = string.Join(" ", parts);
            if (expression.StartsWith("+ ", StringComparison.Ordinal))
                expression = expression.Substring(2);

            return expression;
        }

        private async Task<int> ResolveMiscIdAsync(string miscTypeCode, string code)
        {
            return await (from m in _applicationDbContext.MiscMaster
                          join t in _applicationDbContext.MiscTypeMaster on m.MiscTypeId equals t.Id
                          where t.MiscTypeCode == miscTypeCode
                                && m.Code == code
                                && m.IsDeleted == IsDelete.NotDeleted
                          select m.Id).FirstOrDefaultAsync();
        }
    }
}
