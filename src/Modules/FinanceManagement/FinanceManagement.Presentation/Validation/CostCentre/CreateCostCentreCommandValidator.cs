using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.ICostCentre;
using FinanceManagement.Application.CostCentre.Commands.CreateCostCentre;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.CostCentre
{
    public class CreateCostCentreCommandValidator : AbstractValidator<CreateCostCentreCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICostCentreQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IDepartmentGroupLookup _departmentGroupLookup;
        private readonly IDepartmentLookup _departmentLookup;

        public CreateCostCentreCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ICostCentreQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IDepartmentGroupLookup departmentGroupLookup,
            IDepartmentLookup departmentLookup)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _departmentGroupLookup = departmentGroupLookup;
            _departmentLookup = departmentLookup;

            var maxLengthCode = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.CostCentre>("CostCentreCode") ?? 20;
            var maxLengthName = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.CostCentre>("CostCentreName") ?? 100;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.CostCentreCode)
                            .NotNull().WithMessage($"{nameof(CreateCostCentreCommand.CostCentreCode)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateCostCentreCommand.CostCentreCode)} {rule.Error}");

                        RuleFor(x => x.CostCentreName)
                            .NotNull().WithMessage($"{nameof(CreateCostCentreCommand.CostCentreName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateCostCentreCommand.CostCentreName)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.CostCentreCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateCostCentreCommand.CostCentreCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.CostCentreCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.CostCentreCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateCostCentreCommand.CostCentreCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.CostCentreName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateCostCentreCommand.CostCentreName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "AlreadyExists":
                        // Uniqueness is per unit (the same code is allowed in a different unit).
                        RuleFor(x => x.CostCentreCode)
                            .MustAsync(async (code, ct) =>
                            {
                                var unitId = _ipAddressService.GetUnitId() ?? 0;
                                return !await _queryRepository.AlreadyExistsByCodeAsync(code!, unitId);
                            })
                            .WithMessage($"{nameof(CreateCostCentreCommand.CostCentreCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.CostCentreCode));
                        break;

                    default:
                        break;
                }
            }

            // ── Hierarchy & cross-module FK rules (business-specific — no shared JSON rule applies) ──

            // Level must be a valid COSTCENTRELEVEL misc row.
            RuleFor(x => x.CentreLevelId)
                .GreaterThan(0).WithMessage("Centre Level is required.")
                .MustAsync(async (levelId, ct) => await _queryRepository.GetCentreLevelSortOrderAsync(levelId) > 0)
                .WithMessage("Invalid Centre Level.")
                .When(x => x.CentreLevelId > 0);

            // Only one Plant (L1) cost centre per unit.
            RuleFor(x => x.CentreLevelId)
                .MustAsync(async (levelId, ct) =>
                {
                    var unitId = _ipAddressService.GetUnitId() ?? 0;
                    return !await _queryRepository.PlantExistsForUnitAsync(unitId);
                })
                .WithMessage("A Plant (L1) cost centre already exists for this unit.")
                .WhenAsync(async (x, ct) => await _queryRepository.GetCentreLevelSortOrderAsync(x.CentreLevelId) == 1);

            // Parent must be exactly one level above and in the same unit (null for L1).
            RuleFor(x => x.ParentCostCentreId)
                .MustAsync(async (cmd, parentId, ct) =>
                {
                    var unitId = _ipAddressService.GetUnitId() ?? 0;
                    return await _queryRepository.ParentValidForLevelAsync(parentId, cmd.CentreLevelId, unitId);
                })
                .WithMessage("Invalid Parent Cost Centre — the parent must be exactly one level above and in the same unit.")
                .When(x => x.CentreLevelId > 0);

            // Department Group required for L2 & L3.
            RuleFor(x => x.DepartmentGroupId)
                .MustAsync(async (groupId, ct) =>
                    groupId.HasValue && groupId.Value > 0 && await _departmentGroupLookup.GetByIdAsync(groupId.Value) != null)
                .WithMessage("A valid Department Group is required.")
                .WhenAsync(async (x, ct) => await _queryRepository.GetCentreLevelSortOrderAsync(x.CentreLevelId) >= 2);

            // Department required for L3.
            RuleFor(x => x.DepartmentId)
                .MustAsync(async (deptId, ct) =>
                    deptId.HasValue && deptId.Value > 0 && await _departmentLookup.GetByIdAsync(deptId.Value) != null)
                .WithMessage("A valid Department is required.")
                .WhenAsync(async (x, ct) => await _queryRepository.GetCentreLevelSortOrderAsync(x.CentreLevelId) == 3);
        }
    }
}
