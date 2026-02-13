//using Contracts.Interfaces.Lookups.Workflow;
using BudgetManagement.Application.BudgetRequest.Commands.Create;
using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Domain.Common;
using FluentValidation;

namespace BudgetManagement.Presentation.Validation.BudgetRequest;

public sealed class CreateBudgetRequestCommandValidator : AbstractValidator<CreateBudgetRequestCommand>
{
    private readonly IMiscMasterQueryRepository _miscRepo;
    //private readonly IWorkflowLookup _workflowLookup;
    private readonly IIPAddressService _ipAddressService;
    private readonly IBudgetRequestCommandRepository _budgetRequestCommandRepo;
    private readonly IBudgetRequestQueryRepository _budgetRequestQueryRepo;

    public CreateBudgetRequestCommandValidator(
        IMiscMasterQueryRepository miscRepo,
        //IWorkflowLookup workflowLookup,
        IIPAddressService ipAddressService,
        IBudgetRequestCommandRepository budgetRequestCommandRepo,
        IBudgetRequestQueryRepository budgetRequestQueryRepo)
    {
        _miscRepo = miscRepo;
        //_workflowLookup = workflowLookup;
        _ipAddressService = ipAddressService;
        _budgetRequestCommandRepo = budgetRequestCommandRepo;
        _budgetRequestQueryRepo = budgetRequestQueryRepo;

        // ----------------------------
        // Resolve UnitId from IP (if your system does that)
        // ----------------------------
        var resolvedUnitId = _ipAddressService.GetUnitId();

        // ----------------------------
        // Workflow configured check
        // ----------------------------
        RuleFor(_ => resolvedUnitId)
            .GreaterThan(0)
            .WithMessage("Unit could not be resolved from IP address.");

        //RuleFor(_ => resolvedUnitId)
        //    .MustAsync(async (unitId, ct) =>
        //        await _workflowLookup.IsApproveWorkflowConfigureAsync(MiscEnumEntity.BudgetRequest, unitId, 0))
        //    .WithMessage("Approval Workflow is not configured.");

        // ----------------------------
        // Common mandatory fields
        // ----------------------------
        RuleFor(x => x.UnitId)
            .GreaterThan(0)
            .WithMessage("Unit is required.");

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0)
            .WithMessage("Currency is required.");

        RuleFor(x => x.FinancialYearId)
            .GreaterThan(0)
            .WithMessage("Financial Year is required.");

        RuleFor(x => x.RequestTypeId)
            .GreaterThan(0)
            .WithMessage("Request Type is required.");

        RuleFor(x => x.RequestAmount)
            .GreaterThan(0)
            .WithMessage("Requested Amount must be > 0.");

        RuleFor(x => x.RequestAmount)
            .PrecisionScale(18, 2, true)
            .WithMessage("Requested Amount must have max 2 decimals.");


        RuleFor(x => x.Remarks)
            .MaximumLength(300)
            .WithMessage("Remarks must not exceed 300 characters.");

        RuleFor(x => x.ImagePath)
            .MaximumLength(500)
            .WithMessage("Image Path must not exceed 500 characters.");

        /*  // Dates: required for both OPEX and CAPEX in your unique index scenario
         RuleFor(x => x.FromDate)
             .NotNull()
             .WithMessage("From Date is required.");

         RuleFor(x => x.ToDate)
             .NotNull()
             .WithMessage("To Date is required.");

         RuleFor(x => x)
             .Must(x => x.FromDate == null || x.ToDate == null || x.FromDate <= x.ToDate)
             .WithMessage("From Date must be less than or equal to To Date."); */

        // ----------------------------
        // OPEX specific rules
        // ----------------------------
        WhenAsync(IsOpexAsync, () =>
        {
            RuleFor(x => x.BudgetGroupId)
                .NotNull()
                .GreaterThan(0)
                .WithMessage("Budget Group is required for OPEX.");

            // DB CHECK constraint alignment: OPEX => ProjectId must be NULL
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
        // CAPEX specific rules
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

            // DB CHECK constraint alignment: CAPEX => BudgetGroupId must be NULL
            RuleFor(x => x.BudgetGroupId)
                .Must(v => v is null or 0)
                .WithMessage("Budget Group must be empty for CAPEX.");

            RuleFor(x => x.RequestById)
                .NotNull()
                .GreaterThan(0)
                .WithMessage("Request By is required for CAPEX.");
        });


        // ----------------------------
        // Duplicate check (recommended UX)
        // DB still enforces uniqueness, but this gives clean message before SQL exception
        // ----------------------------
        RuleFor(x => x)
            .MustAsync(NoDuplicateAllocationAsync)
            .WithMessage("A budget Allocation already exists for the selected period.");
        RuleFor(x => x)
            .MustAsync(NoDuplicateAsync)
            .WithMessage("A budget request already exists for the selected period.");     
    }

    private async Task<bool> IsOpexAsync(CreateBudgetRequestCommand cmd, CancellationToken ct)
    {
        var opex = await _miscRepo.GetByTypeAndCodeAsync(
            MiscEnumEntity.BudgetRequestType,
            MiscEnumEntity.Opex,
            ct);

        return opex != null && cmd.RequestTypeId == opex.Id;
    }

    private async Task<bool> IsCapexAsync(CreateBudgetRequestCommand cmd, CancellationToken ct)
    {
        var capex = await _miscRepo.GetByTypeAndCodeAsync(
            MiscEnumEntity.BudgetRequestType,
            MiscEnumEntity.Capex,
            ct);

        return capex != null && cmd.RequestTypeId == capex.Id;
    }
    private async Task<bool> NoDuplicateAllocationAsync(CreateBudgetRequestCommand cmd, CancellationToken ct)
    {
        var exists = !await _budgetRequestQueryRepo.AllocationExistsAsync(
            cmd.FinancialYearId,
            cmd.RequestById ?? 0,
            cmd.RequestMonthId,
            cmd.BudgetGroupId,
            cmd.ProjectId,
            cmd.WBSId,
            ct);

        if (exists == true)
            return false;
        return true;
    }
    private async Task<bool> NoDuplicateAsync(CreateBudgetRequestCommand cmd, CancellationToken ct)
    {
        // If minimum fields missing, skip duplicate check (other rules will fail)
        if (cmd.UnitId <= 0 || cmd.FinancialYearId <= 0 || cmd.RequestTypeId <= 0)
            return true;

        /*   if (cmd.FromDate is null || cmd.ToDate is null)
              return true; */
      

        // Determine uniqueness key based on CAPEX/OPEX rule
        var isCapex = await IsCapexAsync(cmd, ct);
        var isOpex = await IsOpexAsync(cmd, ct);

        if (!isCapex && !isOpex)
            return true; // unknown request type; let other rules handle

        // Normalize nullable ints: treat 0 as null (common UI behavior)
        int? projectId = (cmd.ProjectId is > 0) ? cmd.ProjectId : null;
        int? wbsId = (cmd.WBSId is > 0) ? cmd.WBSId : null;
        int? budgetGroupId = (cmd.BudgetGroupId is > 0) ? cmd.BudgetGroupId : null;
        int? requestById = (cmd.RequestById is > 0) ? cmd.RequestById : null;

        // CAPEX: uniqueness by ProjectId
        if (isCapex)
        {
            if (projectId is null) return true; // will be caught by CAPEX rule
            return !await _budgetRequestCommandRepo.ExistsCapexAsync(
                unitId: cmd.UnitId,
                financialYearId: cmd.FinancialYearId,
                requestTypeId: cmd.RequestTypeId,
                projectId: projectId.Value, wbsId: wbsId ?? 0,
                requestById: requestById,
                ct: ct);
        }

        // OPEX: uniqueness by BudgetGroupId
        if (isOpex)
        {
            if (budgetGroupId is null) return true; // will be caught by OPEX rule
            return !await _budgetRequestCommandRepo
            .ExistsOpexAsync(
                unitId: cmd.UnitId,
                financialYearId: cmd.FinancialYearId,
                requestTypeId: cmd.RequestTypeId,
                budgetGroupId: budgetGroupId.Value,
                requestById: requestById,
                ct: ct);
        }




        return true;
    }
}
