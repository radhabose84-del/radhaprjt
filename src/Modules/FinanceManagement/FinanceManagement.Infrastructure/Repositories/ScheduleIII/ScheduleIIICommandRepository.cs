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

        // ── Header (one row per Company+Division) ────────────────────────────

        // Returns the structure header id for (company, division), creating a DRAFT header if none exists.
        public async Task<int> EnsureHeaderAsync(int companyId, int divisionId)
        {
            var existing = await _applicationDbContext.ScheduleIIIHeader
                .FirstOrDefaultAsync(h => h.CompanyId == companyId && h.DivisionId == divisionId && h.IsDeleted == IsDelete.NotDeleted);

            if (existing != null)
                return existing.Id;

            var header = new ScheduleIIIHeader
            {
                CompanyId = companyId,
                DivisionId = divisionId,
                StatusId = await ResolveMiscIdAsync("S3_STATUS", "DRAFT"),
                TextileSplitEnabled = false
            };
            await _applicationDbContext.ScheduleIIIHeader.AddAsync(header);
            await _applicationDbContext.SaveChangesAsync();
            return header.Id;
        }

        public async Task<int> UpdateHeaderAsync(int companyId, int divisionId, int statusId, bool textileSplitEnabled)
        {
            var header = await _applicationDbContext.ScheduleIIIHeader
                .FirstOrDefaultAsync(h => h.CompanyId == companyId && h.DivisionId == divisionId && h.IsDeleted == IsDelete.NotDeleted);

            if (header == null)
                return 0;

            header.StatusId = statusId;
            header.TextileSplitEnabled = textileSplitEnabled;
            _applicationDbContext.ScheduleIIIHeader.Update(header);
            await _applicationDbContext.SaveChangesAsync();
            return header.Id;
        }

        public async Task<bool> LockStructureAsync(int scheduleIIIHeaderId)
        {
            var lockedStatusId = await ResolveMiscIdAsync("S3_STATUS", "LOCKED");
            if (lockedStatusId == 0)
                return false;

            var header = await _applicationDbContext.ScheduleIIIHeader
                .FirstOrDefaultAsync(h => h.Id == scheduleIIIHeaderId && h.IsDeleted == IsDelete.NotDeleted);

            if (header == null)
                return false;

            header.StatusId = lockedStatusId;
            _applicationDbContext.ScheduleIIIHeader.Update(header);
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }

        // ── Detail (included lines) ──────────────────────────────────────────

        public async Task<int> CreateDetailAsync(ScheduleIIIDetail entity)
        {
            await _applicationDbContext.ScheduleIIIDetail.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateDetailAsync(ScheduleIIIDetail entity)
        {
            var existing = await _applicationDbContext.ScheduleIIIDetail
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.ScheduleIIISectionId = entity.ScheduleIIISectionId;
            existing.ScheduleIIISectionItemId = entity.ScheduleIIISectionItemId;
            existing.DisplayOrder = entity.DisplayOrder;
            existing.IsActive = entity.IsActive;

            _applicationDbContext.ScheduleIIIDetail.Update(existing);
            await _applicationDbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteDetailAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.ScheduleIIIDetail
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.ScheduleIIIDetail.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<int> CreateDetailRangeAsync(List<ScheduleIIIDetail> details)
        {
            if (details == null || details.Count == 0)
                return 0;

            await _applicationDbContext.ScheduleIIIDetail.AddRangeAsync(details);
            await _applicationDbContext.SaveChangesAsync();
            return details.Count;
        }

        public async Task<int> UpdateDetailRangeAsync(List<ScheduleIIIDetail> details)
        {
            if (details == null || details.Count == 0)
                return 0;

            var ids = details.Select(d => d.Id).ToList();
            var existing = await _applicationDbContext.ScheduleIIIDetail
                .Where(x => ids.Contains(x.Id) && x.IsDeleted == IsDelete.NotDeleted)
                .ToListAsync();
            var byId = existing.ToDictionary(x => x.Id);

            // Phase 1 — park every updated row on a temporary negative DisplayOrder so the new
            // orders can't transiently collide with the UNIQUE(HeaderId, DisplayOrder) index.
            foreach (var d in details)
            {
                if (byId.TryGetValue(d.Id, out var ex))
                {
                    ex.DisplayOrder = -ex.Id;
                    _applicationDbContext.ScheduleIIIDetail.Update(ex);
                }
            }
            await _applicationDbContext.SaveChangesAsync();

            // Phase 2 — apply the real values.
            var count = 0;
            foreach (var d in details)
            {
                if (!byId.TryGetValue(d.Id, out var ex))
                    continue;

                ex.ScheduleIIISectionId = d.ScheduleIIISectionId;
                ex.ScheduleIIISectionItemId = d.ScheduleIIISectionItemId;
                ex.DisplayOrder = d.DisplayOrder;
                ex.IsActive = d.IsActive;
                _applicationDbContext.ScheduleIIIDetail.Update(ex);
                count++;
            }
            await _applicationDbContext.SaveChangesAsync();
            return count;
        }

        public async Task<bool> ReorderDetailAsync(int detailId, int direction, CancellationToken ct)
        {
            var row = await _applicationDbContext.ScheduleIIIDetail
                .FirstOrDefaultAsync(x => x.Id == detailId && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (row == null)
                return false;

            // Siblings = same header, excluding self, not deleted.
            var siblings = _applicationDbContext.ScheduleIIIDetail
                .Where(x => x.ScheduleIIIHeaderId == row.ScheduleIIIHeaderId
                         && x.Id != row.Id
                         && x.IsDeleted == IsDelete.NotDeleted);

            ScheduleIIIDetail? neighbour = direction == 1
                ? await siblings.Where(x => x.DisplayOrder < row.DisplayOrder)
                                .OrderByDescending(x => x.DisplayOrder).FirstOrDefaultAsync(ct)
                : await siblings.Where(x => x.DisplayOrder > row.DisplayOrder)
                                .OrderBy(x => x.DisplayOrder).FirstOrDefaultAsync(ct);

            if (neighbour == null)
                return false;

            // Swap display orders without violating the UNIQUE(HeaderId, DisplayOrder) index:
            // park the moving row on a temporary negative order first, then settle both.
            var oldRow = row.DisplayOrder;
            var oldNeighbour = neighbour.DisplayOrder;

            row.DisplayOrder = -row.Id;
            neighbour.DisplayOrder = oldRow;
            _applicationDbContext.ScheduleIIIDetail.Update(row);
            _applicationDbContext.ScheduleIIIDetail.Update(neighbour);
            await _applicationDbContext.SaveChangesAsync(ct);

            row.DisplayOrder = oldNeighbour;
            _applicationDbContext.ScheduleIIIDetail.Update(row);
            await _applicationDbContext.SaveChangesAsync(ct);
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

        // ── Sub-total header (catalog row) — operands handled by SaveSubTotalFormulaAsync ──

        public async Task<int> CreateSubTotalAsync(ScheduleIIISubTotal subTotal)
        {
            // Header only — the operand set (and its derived expression) is saved via SaveSubTotalFormulaAsync.
            subTotal.FormulaExpression = string.Empty;

            await _applicationDbContext.ScheduleIIISubTotal.AddAsync(subTotal);
            await _applicationDbContext.SaveChangesAsync();
            return subTotal.Id;
        }

        public async Task<int> UpdateSubTotalAsync(ScheduleIIISubTotal subTotal)
        {
            var existing = await _applicationDbContext.ScheduleIIISubTotal
                .FirstOrDefaultAsync(x => x.Id == subTotal.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // Header fields only — FormulaExpression is owned by the operand-save path.
            existing.FormulaName = subTotal.FormulaName;
            existing.IncludeOtherIncome = subTotal.IncludeOtherIncome;
            existing.DisplayOrder = subTotal.DisplayOrder;
            existing.IsActive = subTotal.IsActive;

            _applicationDbContext.ScheduleIIISubTotal.Update(existing);
            await _applicationDbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteSubTotalAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.ScheduleIIISubTotal
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.ScheduleIIISubTotal.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }

        // ── Sub-total formula operands (physical replace) ───────────────────
        public async Task<int> SaveSubTotalFormulaAsync(int subTotalId, List<ScheduleIIISubTotalFormula> formulas)
        {
            var existing = await _applicationDbContext.ScheduleIIISubTotal
                .FirstOrDefaultAsync(x => x.Id == subTotalId && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

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

            // The header caches the derived display expression.
            existing.FormulaExpression = await BuildExpressionAsync(formulas);
            _applicationDbContext.ScheduleIIISubTotal.Update(existing);

            await _applicationDbContext.SaveChangesAsync();
            return existing.Id;
        }

        // Rebuilds the cached display string (e.g. "Revenue - Cost of Goods Sold + EBITDA").
        // An operand is either a line item (SectionItemId) or another sub-total (OperandSubTotalId).
        private async Task<string> BuildExpressionAsync(List<ScheduleIIISubTotalFormula> formulas)
        {
            if (formulas == null || formulas.Count == 0)
                return string.Empty;

            var ordered = formulas.OrderBy(f => f.DisplayOrder).ToList();

            var operatorIds = ordered.Select(f => f.OperatorId).Distinct().ToList();
            var operatorCodes = await _applicationDbContext.MiscMaster
                .Where(m => operatorIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => m.Code);

            var sectionItemIds = ordered.Where(f => f.SectionItemId.HasValue)
                .Select(f => f.SectionItemId!.Value).Distinct().ToList();
            var lineNames = await _applicationDbContext.ScheduleIIISectionItem
                .Where(l => sectionItemIds.Contains(l.Id))
                .ToDictionaryAsync(l => l.Id, l => l.LineName);

            var subTotalIds = ordered.Where(f => f.OperandSubTotalId.HasValue)
                .Select(f => f.OperandSubTotalId!.Value).Distinct().ToList();
            var subTotalNames = await _applicationDbContext.ScheduleIIISubTotal
                .Where(s => subTotalIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.FormulaName);

            var parts = new List<string>();
            foreach (var f in ordered)
            {
                var opCode = operatorCodes.TryGetValue(f.OperatorId, out var oc) ? oc : string.Empty;
                var symbol = string.Equals(opCode, "MINUS", StringComparison.OrdinalIgnoreCase) ? "-" : "+";

                string? name = null;
                if (f.SectionItemId.HasValue && lineNames.TryGetValue(f.SectionItemId.Value, out var ln))
                    name = ln;
                else if (f.OperandSubTotalId.HasValue && subTotalNames.TryGetValue(f.OperandSubTotalId.Value, out var sn))
                    name = sn;

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
