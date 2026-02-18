#nullable disable
using BudgetManagement.Application.BudgetRequest.Commands.Update;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Domain.Common;
using FluentValidation;

namespace BudgetManagement.Presentation.Validation.BudgetRequest;

public sealed class UpdateBudgetRequestCommandValidator : AbstractValidator<UpdateBudgetRequestCommand>
{
    private readonly IMiscMasterQueryRepository _miscRepo;
    private readonly IBudgetRequestCommandRepository _budgetRepo;
    private readonly IBudgetRequestQueryRepository _budgetQueryRepo;
    private readonly IBudgetRequestQueryRepository _budgetRequestQueryRepo;

    public UpdateBudgetRequestCommandValidator(
        IBudgetRequestCommandRepository budgetRepo,
        IBudgetRequestQueryRepository budgetQueryRepo,   // ✅ remove if not available
        IMiscMasterQueryRepository miscRepo, IBudgetRequestQueryRepository budgetRequestQueryRepo)
    {
        _budgetRepo = budgetRepo;
        _budgetQueryRepo = budgetQueryRepo;
        _miscRepo = miscRepo;
        _budgetRequestQueryRepo = budgetRequestQueryRepo;

        // ----------------------------
        // Id must exist
        // ----------------------------
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id is required.")
            .MustAsync(async (id, ct) =>
            {
                var entity = await _budgetRepo.GetByIdAsync(id, ct);
                return entity != null;
            })
            .WithMessage("Budget Request not found.");

        // ----------------------------
        // Common mandatory fields
        // ----------------------------
        RuleFor(x => x.UnitId)
            .GreaterThan(0)
            .WithMessage("Unit is required.");

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0)
            .WithMessage("Currency is required.");

        RuleFor(x => x.RequestTypeId)
            .GreaterThan(0)
            .WithMessage("Request Type is required.");

        RuleFor(x => x.RequestAmount)
            .GreaterThan(0)
            .PrecisionScale(18, 2, true)
            .WithMessage("Requested Amount must be > 0 with max 2 decimals.");

        RuleFor(x => x.Remarks)
            .MaximumLength(300)
            .WithMessage("Remarks must not exceed 300 characters.");

        RuleFor(x => x.ImagePath)
            .MaximumLength(500)
            .WithMessage("Image Path must not exceed 500 characters.");

        // ----------------------------
        // Dates (required + range check)
        // ----------------------------
        RuleFor(x => x.FromDate)
            .NotNull()
            .WithMessage("From Date is required.");

        RuleFor(x => x.ToDate)
            .NotNull()
            .WithMessage("To Date is required.");

        RuleFor(x => x)
            .Must(x => x.FromDate == null || x.ToDate == null || x.FromDate <= x.ToDate)
            .WithMessage("From Date must be less than or equal to To Date.");

        // ----------------------------
        // OPEX rules
        // ----------------------------
        WhenAsync(IsOpexAsync, () =>
        {
            RuleFor(x => x.BudgetGroupId)
                .NotNull()
                .GreaterThan(0)
                .WithMessage("Budget Group is required for OPEX.");

            // Align with DB rule: OPEX => ProjectId must be NULL/0
            RuleFor(x => x.ProjectId)
                .Must(v => v is null or 0)
                .WithMessage("Project must be empty for OPEX.");
            RuleFor(x => x.WBSId)
               .Must(v => v is null or 0)
               .WithMessage("Project WBSId must be empty for OPEX.");

            RuleFor(x => x.RequestById)
                .NotNull()
                .GreaterThan(0)
                .WithMessage("Request By is required for OPEX.");
        });

        // ----------------------------
        // CAPEX rules
        // ----------------------------
        WhenAsync(IsCapexAsync, () =>
        {
            RuleFor(x => x.ProjectId)
                .NotNull()
                .GreaterThan(0)
                .WithMessage("Project is required for CAPEX.");

            RuleFor(x => x.WBSId)
                .NotNull()
                .GreaterThan(0)
                .WithMessage("Project WBSId is required for CAPEX.");

            // Align with DB rule: CAPEX => BudgetGroupId must be NULL/0
            RuleFor(x => x.BudgetGroupId)
                .Must(v => v is null or 0)
                .WithMessage("Budget Group must be empty for CAPEX.");

            RuleFor(x => x.RequestById)
                .NotNull()
                .GreaterThan(0)
                .WithMessage("Request By is required for CAPEX.");
        });

        // ----------------------------
        // Duplicate check (recommended UX) - exclude current Id
        // ----------------------------
        RuleFor(x => x)
            .MustAsync(NoDuplicateOnUpdateAsync)
            .WithMessage("A budget request already exists for the selected period.");
    }

    private async Task<bool> IsOpexAsync(UpdateBudgetRequestCommand cmd, CancellationToken ct)
    {
        var opex = await _miscRepo.GetByTypeAndCodeAsync(
            MiscEnumEntity.BudgetRequestType,
            MiscEnumEntity.Opex,
            ct);

        return opex != null && cmd.RequestTypeId == opex.Id;
    }

    private async Task<bool> IsCapexAsync(UpdateBudgetRequestCommand cmd, CancellationToken ct)
    {
        var capex = await _miscRepo.GetByTypeAndCodeAsync(
            MiscEnumEntity.BudgetRequestType,
            MiscEnumEntity.Capex,
            ct);

        return capex != null && cmd.RequestTypeId == capex.Id;
    }

    private async Task<bool> NoDuplicateOnUpdateAsync(UpdateBudgetRequestCommand cmd, CancellationToken ct)
    {
        // If minimum fields missing, skip duplicate check (other rules will fail)
        if (cmd.Id <= 0 || cmd.UnitId <= 0 || cmd.RequestTypeId <= 0)
            return true;

        if (cmd.FromDate is null || cmd.ToDate is null)
            return true;

        // Normalize: treat 0 as null
        int? projectId = (cmd.ProjectId is > 0) ? cmd.ProjectId : null;
        int? wbsId = (cmd.WBSId is > 0) ? cmd.WBSId : null;
        int? budgetGroupId = (cmd.BudgetGroupId is > 0) ? cmd.BudgetGroupId : null;
        int? requestById = (cmd.RequestById is > 0) ? cmd.RequestById : null;

        var isCapex = await IsCapexAsync(cmd, ct);
        var isOpex = await IsOpexAsync(cmd, ct);

        if (isCapex)
        {
            if (projectId is null) return true; // CAPEX rule will catch
            return !await _budgetRepo.ExistsCapexForUpdateAsync(
                excludeId: cmd.Id,
                unitId: cmd.UnitId,
                requestTypeId: cmd.RequestTypeId,
                projectId: projectId.Value,wbsId:wbsId.Value,
                fromDate: cmd.FromDate.Value,
                toDate: cmd.ToDate.Value,
                requestById: requestById,
                ct: ct);
        }

        if (isOpex)
        {
            if (budgetGroupId is null) return true; // OPEX rule will catch
            return !await _budgetRepo.ExistsOpexForUpdateAsync(
                excludeId: cmd.Id,
                unitId: cmd.UnitId,
                requestTypeId: cmd.RequestTypeId,
                budgetGroupId: budgetGroupId.Value,
                fromDate: cmd.FromDate.Value,
                toDate: cmd.ToDate.Value,
                requestById: requestById,
                ct: ct);
        }
        

         var exists =  !await _budgetRequestQueryRepo.AllocationExistsAsync(            
            cmd.FinancialYearId,            
            cmd.RequestById??0,
            cmd.RequestMonthId,
            cmd.BudgetGroupId,            
            cmd.ProjectId,
            cmd.WBSId,            
            ct);           
    
       return !exists;
    }
}
