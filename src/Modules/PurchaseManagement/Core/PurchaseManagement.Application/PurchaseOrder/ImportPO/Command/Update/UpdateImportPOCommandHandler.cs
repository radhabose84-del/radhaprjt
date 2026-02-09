using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Contracts.Interfaces.Lookups.Budget;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Update;

public class UpdateImportPOCommandHandler : IRequestHandler<UpdateImportPOCommand, bool>
{
    private readonly IImportPOCommandRepository _repo;
    private readonly IMapper _mapper;
    private readonly IPODocumentQueryRepository _poDocs;
    private readonly IBudgetAllocationLookup _budgetAllocationLookup;
    private readonly ILogger<UpdateImportPOCommandHandler> _logger;

    public UpdateImportPOCommandHandler(
        IImportPOCommandRepository repo,
        IMapper mapper,
        IPODocumentQueryRepository poDocs,
        IBudgetAllocationLookup budgetAllocationLookup,
        ILogger<UpdateImportPOCommandHandler> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _poDocs = poDocs;
        _budgetAllocationLookup = budgetAllocationLookup ?? throw new ArgumentNullException(nameof(budgetAllocationLookup));
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateImportPOCommand request, CancellationToken ct)
    {
        var dto = request.Data ?? throw new ValidationException("Body required.");
        if (dto.Id <= 0)
            throw new ValidationException("Purchase Order id is required.");

        var existing = await _repo.GetAggregateAsync(dto.Id, ct)
            ?? throw new ValidationException($"Purchase Order {dto.Id} not found.");

        var incoming = _mapper.Map<PurchaseOrderHeader>(dto);
        incoming.Id = dto.Id;

        if (dto.Documents != null && dto.Documents.Count > 0)
        {
            var baseDir = MiscEnumEntity.DocumentPath;
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDir);
            EnsureDirectoryExists(uploadDir);

            foreach (var doc in dto.Documents.Where(d => !string.IsNullOrWhiteSpace(d.FileName)))
            {
                var oldPath = Path.Combine(uploadDir, doc.FileName!);
                if (File.Exists(oldPath))
                {
                    var finalName = $"{dto.PONumber}_{doc.DocumentId}{Path.GetExtension(oldPath)}";
                    var newPath = Path.Combine(uploadDir, finalName);
                    if (!string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
                    {
                        File.Move(oldPath, newPath, overwrite: true);
                        doc.FileName = finalName;
                    }
                }

                if (doc.UploadedDate == default)
                    doc.UploadedDate = DateTimeOffset.UtcNow;
            }
        }

        var updatedId = await _repo.UpdateAsync(incoming, dto, ct);
        if (updatedId > 0)
            await ApplyBudgetAdjustmentsAsync(existing, incoming, updatedId, ct);

        return updatedId > 0;
    }

    private static void EnsureDirectoryExists(string path)
    {
        if (!string.IsNullOrWhiteSpace(path) && !Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    private async Task ApplyBudgetAdjustmentsAsync(
        PurchaseOrderHeader existing,
        PurchaseOrderHeader incoming,
        int poId,
        CancellationToken ct)
    {
        var oldBudgetGroupId = existing.BudgetGroupId ?? 0;
        var newBudgetGroupId = incoming.BudgetGroupId ?? 0;
        if (oldBudgetGroupId <= 0 && newBudgetGroupId <= 0)
            return;

        var oldMonth = new DateOnly(existing.PODate.Year, existing.PODate.Month, 1);
        var newMonth = new DateOnly(incoming.PODate.Year, incoming.PODate.Month, 1);
        var oldRequestById = existing.BudgetRequestById ?? 0;
        var newRequestById = incoming.BudgetRequestById ?? 0;
        var oldMonthId = existing.BudgetMonthId ?? 0;
        var newMonthId = incoming.BudgetMonthId ?? 0;
        var oldProjectId = existing.ProjectId;
        var newProjectId = incoming.ProjectId;
        var oldWbsId = existing.WBSId;
        var newWbsId = incoming.WBSId;
        var oldFinancialYearId = existing.FinancialYearId ?? 0;
        var oldValue = existing.PurchaseValue;
        var newValue = incoming.PurchaseValue;

        if (oldBudgetGroupId == newBudgetGroupId && oldBudgetGroupId > 0)
        {
            var delta = newValue - oldValue;
            if (delta != 0)
            {
                var applied = await _budgetAllocationLookup.ApplyRemainingBalanceDeltaAsync(
                    oldBudgetGroupId,
                    oldMonth,
                    oldRequestById,
                    oldMonthId,
                    delta,
                    oldProjectId,
                    oldWbsId,
                    oldFinancialYearId,
                    ct);

                if (!applied)
                {
                    _logger.LogWarning(
                        "Import PO: Budget delta failed (same BG). PO {PoId}, BG {BgId}, Delta {Delta}",
                        poId,
                        oldBudgetGroupId,
                        delta);
                }
            }

            return;
        }

        if (oldBudgetGroupId > 0 && oldValue != 0)
        {
            var reverted = await _budgetAllocationLookup.ApplyRemainingBalanceDeltaAsync(
                oldBudgetGroupId,
                oldMonth,
                oldRequestById,
                oldMonthId,
                -oldValue,
                oldProjectId,
                oldWbsId,
                oldFinancialYearId,
                ct);

            if (!reverted)
            {
                _logger.LogWarning(
                    "Import PO: Budget refund failed (old BG). PO {PoId}, BG {BgId}, Delta {Delta}",
                    poId,
                    oldBudgetGroupId,
                    -oldValue);
            }
        }

        if (newBudgetGroupId > 0 && newValue != 0)
        {
            var applied = await _budgetAllocationLookup.ApplyRemainingBalanceDeltaAsync(
                newBudgetGroupId,
                newMonth,
                newRequestById,
                newMonthId,
                newValue,
                newProjectId,
                newWbsId,
                oldFinancialYearId,
                ct);

            if (!applied)
            {
                _logger.LogWarning(
                    "Import PO: Budget consume failed (new BG). PO {PoId}, BG {BgId}, Delta {Delta}",
                    poId,
                    newBudgetGroupId,
                    newValue);
            }
        }
    }
}
