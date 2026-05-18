using BudgetManagement.Application.BudgetGroups.Commands.CreateBudgetGroup;
using BudgetManagement.Application.Common.Interfaces.IBudgetGroupMaster;
using FluentValidation;
using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Domain.Common;


namespace BudgetManagement.Presentation.Validation.BudgetGroup
{
    public class CreateBudgetGroupCommandValidator : AbstractValidator<CreateBudgetGroupCommand>
    {
        public CreateBudgetGroupCommandValidator(
            IBudgetGroupCommandRepository repo,
            IBudgetGroupQueryRepository queryRepo,
            IMiscMasterQueryRepository miscMasterQueryRepo)
        {
            // Fetch the AllocationRule IDs dynamically from MiscMaster using MiscEnumEntity
            var allocationRuleByPercentageId = miscMasterQueryRepo.GetMiscMasterByName(MiscEnumEntity.AllocationType, MiscEnumEntity.AllocationTypePercentage).Result?.Id;
            var allocationRuleBySpindleId = miscMasterQueryRepo.GetMiscMasterByName(MiscEnumEntity.AllocationType, MiscEnumEntity.AllocationTypeSpindle).Result?.Id;
            var allocationRuleByRequestId = miscMasterQueryRepo.GetMiscMasterByName(MiscEnumEntity.AllocationType, MiscEnumEntity.AllocationTypeRequest).Result?.Id;
            var budgetTypeAnnualId  = miscMasterQueryRepo.GetMiscMasterByName(MiscEnumEntity.BudgetType, MiscEnumEntity.BudgetTypeAnnual).Result?.Id;
            var budgetTypeMonthlyId = miscMasterQueryRepo.GetMiscMasterByName(MiscEnumEntity.BudgetType, MiscEnumEntity.BudgetTypeMonthly).Result?.Id;

            // ---------- Basic fields ----------
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Budget Group Name is required.")
                .MaximumLength(100).WithMessage("Budget Group Name must not exceed 100 characters.")
                .MustAsync(async (cmd, name, ct) =>
                    !await repo.IsNameDuplicateAsync(
                        name!,          // name
                        0,              // excludeId = 0 for create
                        cmd.UnitId,
                        cmd.DepartmentId,
                        ct))
                .WithMessage("Budget Group name already exists for this Unit and Department.");

            RuleFor(x => x.UnitId)
                .GreaterThan(0).WithMessage("Unit is required.");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("Department is required.");

            RuleFor(x => x.CostCenterId)
                .GreaterThan(0).WithMessage("Cost Center is required.");

            RuleFor(x => x.CurrencyId)
                .GreaterThan(0).WithMessage("Currency is required.");

            RuleFor(x => x.IsActive)
                .NotNull().WithMessage("Status is required.");

            // AllocationRuleId: allow null/0 or valid ids
            RuleFor(x => x.AllocationRuleId)
                .Must(id =>
                    id == null
                    || id == 0
                    || (allocationRuleByPercentageId.HasValue && id == allocationRuleByPercentageId)
                    || (allocationRuleBySpindleId.HasValue && id == allocationRuleBySpindleId)
                    || (allocationRuleByRequestId.HasValue && id == allocationRuleByRequestId))
                .WithMessage("Invalid Allocation Rule.");

            // ---------- Allocation: generic range guards ----------
            RuleFor(x => x.AllocatedPercentage)
                .InclusiveBetween(0.01m, 100m)
                .When(x => x.AllocatedPercentage.HasValue && x.AllocatedPercentage.Value > 0)
                .WithMessage("AllocatedPercentage must be between 0.01 and 100.");

            RuleFor(x => x.AllocatedSpindleCost)
                .GreaterThan(0)
                .When(x => x.AllocatedSpindleCost.HasValue && x.AllocatedSpindleCost.Value > 0)
                .WithMessage("AllocatedSpindleCost must be greater than 0.");

            // ---------- Allocation: rule-specific requirements ----------

            // PERCENTAGE → percentage required, spindle must be null/0
            When(x => allocationRuleByPercentageId.HasValue && x.AllocationRuleId == allocationRuleByPercentageId, () =>
            {
                RuleFor(x => x.AllocatedPercentage)
                    .NotNull().WithMessage("AllocatedPercentage is required when Allocation Rule is PERCENTAGE.")
                    .GreaterThan(0).WithMessage("AllocatedPercentage must be greater than 0 when Allocation Rule is PERCENTAGE.");

                RuleFor(x => x.AllocatedSpindleCost)
                    .Must(v => !v.HasValue || v.Value == 0)
                    .WithMessage("AllocatedSpindleCost must be zero when Allocation Rule is PERCENTAGE.");
            });

            // BY_SPINDLE → spindle cost required, percentage MUST be empty
            When(x => allocationRuleBySpindleId.HasValue && x.AllocationRuleId == allocationRuleBySpindleId, () =>
            {
                RuleFor(x => x.AllocatedSpindleCost)
                    .NotNull().WithMessage("AllocatedSpindleCost is required when Allocation Rule is SPINDLE.")
                    .GreaterThan(0).WithMessage("AllocatedSpindleCost must be greater than 0 when Allocation Rule is SPINDLE.");

                RuleFor(x => x.AllocatedPercentage)
                    .Must(v => !v.HasValue || v.Value == 0)
                    .WithMessage("AllocatedPercentage must be zero when Allocation Rule is SPINDLE.");
            });

            // REQUEST → no allocation fields required (allow null or 0)
            When(x => allocationRuleByRequestId.HasValue && x.AllocationRuleId == allocationRuleByRequestId, () =>
            {
                RuleFor(x => x.AllocatedPercentage)
                    .Must(v => !v.HasValue || v.Value == 0)
                    .WithMessage("AllocatedPercentage must be zero when Allocation Rule is REQUEST.");

                RuleFor(x => x.AllocatedSpindleCost)
                    .Must(v => !v.HasValue || v.Value == 0)
                    .WithMessage("AllocatedSpindleCost must be zero when Allocation Rule is REQUEST.");
            });

            RuleFor(x => x.BudgetTypeId)
                .GreaterThan(0).WithMessage("Budget Type is required.")
                .Must(id =>
                  (budgetTypeAnnualId.HasValue && id == budgetTypeAnnualId) ||
                  (budgetTypeMonthlyId.HasValue && id == budgetTypeMonthlyId))
                .WithMessage("Invalid Budget Type.");

            // Disallow both > 0 together (important fix)
            RuleFor(x => x)
                .Must(x =>
                    !(x.AllocatedPercentage.GetValueOrDefault() > 0 &&
                      x.AllocatedSpindleCost.GetValueOrDefault() > 0))
                .WithMessage("Only one of AllocatedPercentage or AllocatedSpindleCost can be provided.");

        }
    }
}
