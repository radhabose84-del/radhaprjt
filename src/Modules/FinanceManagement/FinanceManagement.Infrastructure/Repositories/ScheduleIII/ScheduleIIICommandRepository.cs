using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Dto;
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

        // ── Composite aggregate (all five tables in one call) ────────────────

        public async Task<int> CreateAggregateAsync(ScheduleIIIInput input)
        {
            var structure = new ScheduleIIIStructure
            {
                CompanyId = input.CompanyId,
                DivisionId = input.DivisionId,
                StructureStatusId = input.StructureStatusId,
                TextileSplitEnabled = input.TextileSplitEnabled == 1,
                VersionNo = input.VersionNo <= 0 ? 1 : input.VersionNo
            };
            await _applicationDbContext.ScheduleIIIStructure.AddAsync(structure);
            await _applicationDbContext.SaveChangesAsync();

            await PersistChildrenAsync(structure.Id, input);
            return structure.Id;
        }

        public async Task<int> UpdateAggregateAsync(ScheduleIIIInput input)
        {
            var structure = await _applicationDbContext.ScheduleIIIStructure
                .FirstOrDefaultAsync(x => x.Id == input.Id && x.IsDeleted == IsDelete.NotDeleted);

            if (structure == null)
                return 0;

            // CompanyId/DivisionId are immutable.
            structure.StructureStatusId = input.StructureStatusId;
            structure.TextileSplitEnabled = input.TextileSplitEnabled == 1;
            structure.VersionNo = input.VersionNo <= 0 ? structure.VersionNo : input.VersionNo;
            structure.IsActive = input.IsActive == 1 ? Status.Active : Status.Inactive;
            _applicationDbContext.ScheduleIIIStructure.Update(structure);
            await _applicationDbContext.SaveChangesAsync();

            // In-place upsert: child rows with Id > 0 are updated, Id == 0 are inserted. Nothing is deleted.
            await UpsertChildrenAsync(input.Id, input);
            return structure.Id;
        }

        private async Task PersistChildrenAsync(int structureId, ScheduleIIIInput input)
        {
            var lineByCode = new Dictionary<string, ScheduleIIILineItem>(StringComparer.OrdinalIgnoreCase);

            // Sections + line items
            foreach (var secIn in input.Sections)
            {
                var section = new ScheduleIIISection
                {
                    StructureId = structureId,
                    SectionName = secIn.SectionName,
                    StatementTypeId = secIn.StatementTypeId,
                    NatureId = secIn.NatureId,
                    DisplayOrder = secIn.DisplayOrder
                };
                await _applicationDbContext.ScheduleIIISection.AddAsync(section);
                await _applicationDbContext.SaveChangesAsync();

                foreach (var lineIn in secIn.LineItems)
                {
                    var line = new ScheduleIIILineItem
                    {
                        StructureId = structureId,
                        SectionId = section.Id,
                        LineCode = lineIn.LineCode,
                        LineName = lineIn.LineName,
                        SubClassification = lineIn.SubClassification,
                        NoteReference = lineIn.NoteReference,
                        DisplayOrder = lineIn.DisplayOrder,
                        IsSplitLine = lineIn.IsSplitLine == 1
                    };
                    await _applicationDbContext.ScheduleIIILineItem.AddAsync(line);
                    await _applicationDbContext.SaveChangesAsync();

                    if (!string.IsNullOrWhiteSpace(line.LineCode))
                        lineByCode[line.LineCode!] = line;
                }
            }

            // Resolve parent/child links by LineCode.
            foreach (var secIn in input.Sections)
            {
                foreach (var lineIn in secIn.LineItems)
                {
                    if (!string.IsNullOrWhiteSpace(lineIn.ParentLineCode)
                        && !string.IsNullOrWhiteSpace(lineIn.LineCode)
                        && lineByCode.TryGetValue(lineIn.LineCode!, out var child)
                        && lineByCode.TryGetValue(lineIn.ParentLineCode!, out var parent))
                    {
                        child.ParentLineId = parent.Id;
                        _applicationDbContext.ScheduleIIILineItem.Update(child);
                    }
                }
            }
            await _applicationDbContext.SaveChangesAsync();

            // Sub-totals
            var subTotalByName = new Dictionary<string, ScheduleIIISubTotal>(StringComparer.OrdinalIgnoreCase);
            foreach (var subIn in input.SubTotals)
            {
                var sub = new ScheduleIIISubTotal
                {
                    StructureId = structureId,
                    SubTotalName = subIn.SubTotalName,
                    FormulaExpression = string.Empty,
                    IncludeOtherIncome = subIn.IncludeOtherIncome == 1,
                    IsSystemDefined = false,
                    DisplayOrder = subIn.DisplayOrder
                };
                await _applicationDbContext.ScheduleIIISubTotal.AddAsync(sub);
                await _applicationDbContext.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(sub.SubTotalName))
                    subTotalByName[sub.SubTotalName!] = sub;
            }

            // Formulas (operands resolved by code: LineCode for lines, SubTotalName for sub-totals)
            var subTotalOperandTypeId = await ResolveMiscIdAsync("S3_OPERAND_TYPE", "SUBTOTAL");
            foreach (var subIn in input.SubTotals)
            {
                if (string.IsNullOrWhiteSpace(subIn.SubTotalName)
                    || !subTotalByName.TryGetValue(subIn.SubTotalName!, out var sub))
                    continue;

                var formulas = new List<ScheduleIIISubTotalFormula>();
                foreach (var fIn in subIn.Formulas)
                {
                    var operandRefId = 0;
                    if (fIn.OperandTypeId == subTotalOperandTypeId)
                    {
                        if (!string.IsNullOrWhiteSpace(fIn.OperandCode) && subTotalByName.TryGetValue(fIn.OperandCode!, out var refSub))
                            operandRefId = refSub.Id;
                    }
                    else if (!string.IsNullOrWhiteSpace(fIn.OperandCode) && lineByCode.TryGetValue(fIn.OperandCode!, out var refLine))
                    {
                        operandRefId = refLine.Id;
                    }

                    formulas.Add(new ScheduleIIISubTotalFormula
                    {
                        SubTotalId = sub.Id,
                        OperandTypeId = fIn.OperandTypeId,
                        OperandRefId = operandRefId,
                        OperatorId = fIn.OperatorId,
                        DisplayOrder = fIn.DisplayOrder
                    });
                }

                await _applicationDbContext.ScheduleIIISubTotalFormula.AddRangeAsync(formulas);
                await _applicationDbContext.SaveChangesAsync();

                sub.FormulaExpression = await BuildExpressionAsync(formulas);
                _applicationDbContext.ScheduleIIISubTotal.Update(sub);
            }
            await _applicationDbContext.SaveChangesAsync();
        }

        // In-place upsert of the structure's children: Id > 0 updates that row, Id == 0 inserts. No deletes.
        private async Task UpsertChildrenAsync(int structureId, ScheduleIIIInput input)
        {
            var lineByCode = new Dictionary<string, ScheduleIIILineItem>(StringComparer.OrdinalIgnoreCase);

            // Pre-load existing lines so parent/operand codes resolve against rows not in the payload too.
            var existingLines = await _applicationDbContext.ScheduleIIILineItem
                .Where(x => x.StructureId == structureId && x.IsDeleted == IsDelete.NotDeleted).ToListAsync();
            foreach (var l in existingLines)
                if (!string.IsNullOrWhiteSpace(l.LineCode)) lineByCode[l.LineCode!] = l;

            // Pre-load existing sections for natural-key (SectionName) matching.
            var existingSections = await _applicationDbContext.ScheduleIIISection
                .Where(x => x.StructureId == structureId && x.IsDeleted == IsDelete.NotDeleted).ToListAsync();
            var sectionByName = new Dictionary<string, ScheduleIIISection>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in existingSections)
                if (!string.IsNullOrWhiteSpace(s.SectionName)) sectionByName[s.SectionName!] = s;

            // Sections + line items
            foreach (var secIn in input.Sections)
            {
                // Match by Id (if supplied) else by natural key (SectionName) — update; otherwise insert.
                ScheduleIIISection? section = null;
                if (secIn.Id > 0)
                    section = existingSections.FirstOrDefault(x => x.Id == secIn.Id);
                else if (!string.IsNullOrWhiteSpace(secIn.SectionName) && sectionByName.TryGetValue(secIn.SectionName!, out var foundSec))
                    section = foundSec;

                if (section != null)
                {
                    section.SectionName = secIn.SectionName;
                    section.StatementTypeId = secIn.StatementTypeId;
                    section.NatureId = secIn.NatureId;
                    section.DisplayOrder = secIn.DisplayOrder;
                    _applicationDbContext.ScheduleIIISection.Update(section);
                }
                else
                {
                    section = new ScheduleIIISection
                    {
                        StructureId = structureId,
                        SectionName = secIn.SectionName,
                        StatementTypeId = secIn.StatementTypeId,
                        NatureId = secIn.NatureId,
                        DisplayOrder = secIn.DisplayOrder
                    };
                    await _applicationDbContext.ScheduleIIISection.AddAsync(section);
                }
                await _applicationDbContext.SaveChangesAsync();
                if (!string.IsNullOrWhiteSpace(section.SectionName)) sectionByName[section.SectionName!] = section;

                foreach (var lineIn in secIn.LineItems)
                {
                    // Match by Id (if supplied) else by natural key (LineCode) — update; otherwise insert.
                    ScheduleIIILineItem? line = null;
                    if (lineIn.Id > 0)
                        line = existingLines.FirstOrDefault(x => x.Id == lineIn.Id);
                    else if (!string.IsNullOrWhiteSpace(lineIn.LineCode) && lineByCode.TryGetValue(lineIn.LineCode!, out var foundLine))
                        line = foundLine;

                    if (line != null)
                    {
                        line.SectionId = section.Id;
                        line.LineCode = lineIn.LineCode;
                        line.LineName = lineIn.LineName;
                        line.SubClassification = lineIn.SubClassification;
                        line.NoteReference = lineIn.NoteReference;
                        line.DisplayOrder = lineIn.DisplayOrder;
                        line.IsSplitLine = lineIn.IsSplitLine == 1;
                        _applicationDbContext.ScheduleIIILineItem.Update(line);
                    }
                    else
                    {
                        line = new ScheduleIIILineItem
                        {
                            StructureId = structureId,
                            SectionId = section.Id,
                            LineCode = lineIn.LineCode,
                            LineName = lineIn.LineName,
                            SubClassification = lineIn.SubClassification,
                            NoteReference = lineIn.NoteReference,
                            DisplayOrder = lineIn.DisplayOrder,
                            IsSplitLine = lineIn.IsSplitLine == 1
                        };
                        await _applicationDbContext.ScheduleIIILineItem.AddAsync(line);
                    }
                    await _applicationDbContext.SaveChangesAsync();

                    if (!string.IsNullOrWhiteSpace(line.LineCode)) lineByCode[line.LineCode!] = line;
                }
            }

            // Resolve parent/child links by LineCode.
            foreach (var secIn in input.Sections)
            {
                foreach (var lineIn in secIn.LineItems)
                {
                    if (!string.IsNullOrWhiteSpace(lineIn.ParentLineCode)
                        && !string.IsNullOrWhiteSpace(lineIn.LineCode)
                        && lineByCode.TryGetValue(lineIn.LineCode!, out var child)
                        && lineByCode.TryGetValue(lineIn.ParentLineCode!, out var parent))
                    {
                        child.ParentLineId = parent.Id;
                        _applicationDbContext.ScheduleIIILineItem.Update(child);
                    }
                }
            }
            await _applicationDbContext.SaveChangesAsync();

            // Sub-totals
            var subByName = new Dictionary<string, ScheduleIIISubTotal>(StringComparer.OrdinalIgnoreCase);
            var existingSubs = await _applicationDbContext.ScheduleIIISubTotal
                .Where(x => x.StructureId == structureId && x.IsDeleted == IsDelete.NotDeleted).ToListAsync();
            foreach (var s in existingSubs)
                if (!string.IsNullOrWhiteSpace(s.SubTotalName)) subByName[s.SubTotalName!] = s;

            foreach (var subIn in input.SubTotals)
            {
                // Match by Id (if supplied) else by natural key (SubTotalName) — update; otherwise insert.
                ScheduleIIISubTotal? sub = null;
                if (subIn.Id > 0)
                    sub = existingSubs.FirstOrDefault(x => x.Id == subIn.Id);
                else if (!string.IsNullOrWhiteSpace(subIn.SubTotalName) && subByName.TryGetValue(subIn.SubTotalName!, out var foundSub))
                    sub = foundSub;

                if (sub != null)
                {
                    sub.SubTotalName = subIn.SubTotalName;
                    sub.IncludeOtherIncome = subIn.IncludeOtherIncome == 1;
                    sub.DisplayOrder = subIn.DisplayOrder;
                    _applicationDbContext.ScheduleIIISubTotal.Update(sub);
                }
                else
                {
                    sub = new ScheduleIIISubTotal
                    {
                        StructureId = structureId,
                        SubTotalName = subIn.SubTotalName,
                        FormulaExpression = string.Empty,
                        IncludeOtherIncome = subIn.IncludeOtherIncome == 1,
                        IsSystemDefined = false,
                        DisplayOrder = subIn.DisplayOrder
                    };
                    await _applicationDbContext.ScheduleIIISubTotal.AddAsync(sub);
                }
                await _applicationDbContext.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(sub.SubTotalName)) subByName[sub.SubTotalName!] = sub;
            }

            // Existing formulas keyed by (SubTotalId -> DisplayOrder) for natural-key matching.
            var subIdsAll = existingSubs.Select(s => s.Id).ToList();
            var existingFormulas = await _applicationDbContext.ScheduleIIISubTotalFormula
                .Where(f => subIdsAll.Contains(f.SubTotalId) && f.IsDeleted == IsDelete.NotDeleted).ToListAsync();
            var formulaBySub = new Dictionary<int, Dictionary<int, ScheduleIIISubTotalFormula>>();
            foreach (var f in existingFormulas)
            {
                if (!formulaBySub.TryGetValue(f.SubTotalId, out var byOrder)) { byOrder = new(); formulaBySub[f.SubTotalId] = byOrder; }
                byOrder[f.DisplayOrder] = f;
            }

            // Formulas (operands resolved by code), then rebuild each sub-total's expression.
            var subTotalOperandTypeId = await ResolveMiscIdAsync("S3_OPERAND_TYPE", "SUBTOTAL");
            foreach (var subIn in input.SubTotals)
            {
                if (string.IsNullOrWhiteSpace(subIn.SubTotalName)
                    || !subByName.TryGetValue(subIn.SubTotalName!, out var sub))
                    continue;

                formulaBySub.TryGetValue(sub.Id, out var existingByOrder);
                var formulasForExpr = new List<ScheduleIIISubTotalFormula>();
                foreach (var fIn in subIn.Formulas)
                {
                    var operandRefId = 0;
                    if (fIn.OperandTypeId == subTotalOperandTypeId)
                    {
                        if (!string.IsNullOrWhiteSpace(fIn.OperandCode) && subByName.TryGetValue(fIn.OperandCode!, out var refSub))
                            operandRefId = refSub.Id;
                    }
                    else if (!string.IsNullOrWhiteSpace(fIn.OperandCode) && lineByCode.TryGetValue(fIn.OperandCode!, out var refLine))
                    {
                        operandRefId = refLine.Id;
                    }

                    // Match by Id (if supplied) else by natural key (DisplayOrder within the sub-total) — update; otherwise insert.
                    ScheduleIIISubTotalFormula? formula = null;
                    if (fIn.Id > 0)
                        formula = existingFormulas.FirstOrDefault(x => x.Id == fIn.Id);
                    else if (existingByOrder != null && existingByOrder.TryGetValue(fIn.DisplayOrder, out var foundF))
                        formula = foundF;

                    if (formula != null)
                    {
                        formula.SubTotalId = sub.Id;
                        formula.OperandTypeId = fIn.OperandTypeId;
                        formula.OperandRefId = operandRefId;
                        formula.OperatorId = fIn.OperatorId;
                        formula.DisplayOrder = fIn.DisplayOrder;
                        _applicationDbContext.ScheduleIIISubTotalFormula.Update(formula);
                    }
                    else
                    {
                        formula = new ScheduleIIISubTotalFormula
                        {
                            SubTotalId = sub.Id,
                            OperandTypeId = fIn.OperandTypeId,
                            OperandRefId = operandRefId,
                            OperatorId = fIn.OperatorId,
                            DisplayOrder = fIn.DisplayOrder
                        };
                        await _applicationDbContext.ScheduleIIISubTotalFormula.AddAsync(formula);
                    }
                    formulasForExpr.Add(formula);
                }
                await _applicationDbContext.SaveChangesAsync();

                sub.FormulaExpression = await BuildExpressionAsync(formulasForExpr);
                _applicationDbContext.ScheduleIIISubTotal.Update(sub);
            }
            await _applicationDbContext.SaveChangesAsync();
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
