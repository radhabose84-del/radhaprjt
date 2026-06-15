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

        public async Task<int> CreateLineItemAsync(ScheduleIIILineItem entity)
        {
            await _applicationDbContext.ScheduleIIILineItem.AddAsync(entity);
            await _applicationDbContext.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> UpdateLineItemAsync(ScheduleIIILineItem entity)
        {
            var existing = await _applicationDbContext.ScheduleIIILineItem
                .FirstOrDefaultAsync(x => x.Id == entity.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            // LineCode is immutable — only mutable fields are updated.
            existing.LineName = entity.LineName;
            existing.SubClassification = entity.SubClassification;
            existing.NoteReference = entity.NoteReference;
            existing.DisplayOrder = entity.DisplayOrder;
            existing.IsActive = entity.IsActive;

            _applicationDbContext.ScheduleIIILineItem.Update(existing);
            await _applicationDbContext.SaveChangesAsync();
            return existing.Id;
        }

        public async Task<bool> SoftDeleteLineItemAsync(int id, CancellationToken ct)
        {
            var existing = await _applicationDbContext.ScheduleIIILineItem
                .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (existing == null)
                return false;

            existing.IsDeleted = IsDelete.Deleted;
            _applicationDbContext.ScheduleIIILineItem.Update(existing);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> ReorderLineItemAsync(int lineItemId, int direction, CancellationToken ct)
        {
            var line = await _applicationDbContext.ScheduleIIILineItem
                .FirstOrDefaultAsync(x => x.Id == lineItemId && x.IsDeleted == IsDelete.NotDeleted, ct);

            if (line == null)
                return false;

            // Siblings = same section + same parent (NULL-safe), excluding self, not deleted.
            var siblings = _applicationDbContext.ScheduleIIILineItem
                .Where(x => x.SectionId == line.SectionId
                         && x.ParentLineId == line.ParentLineId
                         && x.Id != line.Id
                         && x.IsDeleted == IsDelete.NotDeleted);

            ScheduleIIILineItem? neighbour = direction == 1
                ? await siblings.Where(x => x.DisplayOrder < line.DisplayOrder)
                                .OrderByDescending(x => x.DisplayOrder).FirstOrDefaultAsync(ct)
                : await siblings.Where(x => x.DisplayOrder > line.DisplayOrder)
                                .OrderBy(x => x.DisplayOrder).FirstOrDefaultAsync(ct);

            if (neighbour == null)
                return false;

            (line.DisplayOrder, neighbour.DisplayOrder) = (neighbour.DisplayOrder, line.DisplayOrder);

            _applicationDbContext.ScheduleIIILineItem.Update(line);
            _applicationDbContext.ScheduleIIILineItem.Update(neighbour);
            await _applicationDbContext.SaveChangesAsync(ct);
            return true;
        }

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

        public async Task<int> UpdateSubTotalAsync(int subTotalId, string? subTotalName, bool includeOtherIncome, List<ScheduleIIISubTotalFormula> formulas)
        {
            var existing = await _applicationDbContext.ScheduleIIISubTotal
                .FirstOrDefaultAsync(x => x.Id == subTotalId && x.IsDeleted == IsDelete.NotDeleted);

            if (existing == null)
                return 0;

            existing.SubTotalName = subTotalName;
            existing.IncludeOtherIncome = includeOtherIncome;
            existing.FormulaExpression = await BuildExpressionAsync(formulas);

            // Replace the operand set: soft-delete the old rows, insert the new ones.
            var oldFormulas = await _applicationDbContext.ScheduleIIISubTotalFormula
                .Where(f => f.SubTotalId == subTotalId && f.IsDeleted == IsDelete.NotDeleted)
                .ToListAsync();

            foreach (var f in oldFormulas)
                f.IsDeleted = IsDelete.Deleted;

            foreach (var f in formulas)
                f.SubTotalId = subTotalId;

            await _applicationDbContext.ScheduleIIISubTotalFormula.AddRangeAsync(formulas);
            _applicationDbContext.ScheduleIIISubTotal.Update(existing);
            await _applicationDbContext.SaveChangesAsync();

            return existing.Id;
        }

        public async Task<bool> LockStructureAsync(int structureId)
        {
            var lockedStatusId = await ResolveMiscIdAsync("S3_STATUS", "LOCKED");
            if (lockedStatusId == 0)
                return false;

            var structure = await _applicationDbContext.ScheduleIIIStructure
                .FirstOrDefaultAsync(x => x.Id == structureId && x.IsDeleted == IsDelete.NotDeleted);

            if (structure == null)
                return false;

            structure.StructureStatusId = lockedStatusId;
            _applicationDbContext.ScheduleIIIStructure.Update(structure);
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }

        // Rebuilds the cached display string (e.g. "Gross Profit + Other Income - Operating Expenses").
        private async Task<string> BuildExpressionAsync(List<ScheduleIIISubTotalFormula> formulas)
        {
            if (formulas == null || formulas.Count == 0)
                return string.Empty;

            var ordered = formulas.OrderBy(f => f.DisplayOrder).ToList();

            var miscIds = ordered.Select(f => f.OperatorId)
                .Concat(ordered.Select(f => f.OperandTypeId))
                .Distinct()
                .ToList();
            var miscCodes = await _applicationDbContext.MiscMaster
                .Where(m => miscIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => m.Code);

            var refIds = ordered.Select(f => f.OperandRefId).Distinct().ToList();
            var lineNames = await _applicationDbContext.ScheduleIIILineItem
                .Where(l => refIds.Contains(l.Id))
                .ToDictionaryAsync(l => l.Id, l => l.LineName);
            var subNames = await _applicationDbContext.ScheduleIIISubTotal
                .Where(s => refIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.SubTotalName);

            var parts = new List<string>();
            foreach (var f in ordered)
            {
                var opCode = miscCodes.TryGetValue(f.OperatorId, out var oc) ? oc : string.Empty;
                var symbol = string.Equals(opCode, "MINUS", StringComparison.OrdinalIgnoreCase) ? "-" : "+";

                var typeCode = miscCodes.TryGetValue(f.OperandTypeId, out var tc) ? tc : string.Empty;
                var name = string.Equals(typeCode, "SUBTOTAL", StringComparison.OrdinalIgnoreCase)
                    ? (subNames.TryGetValue(f.OperandRefId, out var sn) ? sn : null)
                    : (lineNames.TryGetValue(f.OperandRefId, out var ln) ? ln : null);

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
