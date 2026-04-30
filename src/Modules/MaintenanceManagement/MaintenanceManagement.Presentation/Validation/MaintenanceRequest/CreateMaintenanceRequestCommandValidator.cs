using Contracts.Interfaces.Lookups.Users;
using FluentValidation;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Command.CreateMaintenanceRequest;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.MaintenanceRequest
{
    public class CreateMaintenanceRequestCommandValidator : AbstractValidator<CreateMaintenanceRequestCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMaintenanceRequestQueryRepository _maintenanceRequestQueryRepository;
        private readonly IDepartmentLookup _departmentLookup;

        public CreateMaintenanceRequestCommandValidator(
            IMaintenanceRequestQueryRepository maintenanceRequestQueryRepository,
            IDepartmentLookup departmentLookup)
        {
            _maintenanceRequestQueryRepository = maintenanceRequestQueryRepository;
            _departmentLookup = departmentLookup;

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
                        RuleFor(x => x.MaintenanceTypeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMaintenanceRequestCommand.MaintenanceTypeId)} {rule.Error}");

                        RuleFor(x => x.MachineId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMaintenanceRequestCommand.MachineId)} {rule.Error}");

                        RuleFor(x => x.RequestTypeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMaintenanceRequestCommand.RequestTypeId)} {rule.Error}");

                        RuleFor(x => x.MachineId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateMaintenanceRequestCommand.MachineId)} {rule.Error}");

                        RuleFor(x => x.RequestTypeId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateMaintenanceRequestCommand.RequestTypeId)} {rule.Error}");

                        break;

                    case "FKColumnDelete":
                        // Same-module FK: validate machine exists, is active, not deleted.
                        RuleFor(x => x.MachineId)
                            .MustAsync(async (id, ct) => await _maintenanceRequestQueryRepository.MachineExistsAsync(id))
                            .WithMessage($"{nameof(CreateMaintenanceRequestCommand.MachineId)} {rule.Error}")
                            .When(x => x.MachineId > 0);

                        // Cross-module FK to UserManagement.Department — go via lookup interface,
                        // never via SQL JOIN. ProductionDepartmentId / MaintenanceDepartmentId are
                        // optional (may be 0), so guard with .When().
                        RuleFor(x => x.ProductionDepartmentId)
                            .MustAsync(async (id, ct) =>
                            {
                                var dept = await _departmentLookup.GetByIdAsync(id, ct);
                                return dept != null;
                            })
                            .WithMessage($"{nameof(CreateMaintenanceRequestCommand.ProductionDepartmentId)} {rule.Error}")
                            .When(x => x.ProductionDepartmentId > 0);

                        RuleFor(x => x.MaintenanceDepartmentId)
                            .MustAsync(async (id, ct) =>
                            {
                                var dept = await _departmentLookup.GetByIdAsync(id, ct);
                                return dept != null;
                            })
                            .WithMessage($"{nameof(CreateMaintenanceRequestCommand.MaintenanceDepartmentId)} {rule.Error}")
                            .When(x => x.MaintenanceDepartmentId > 0);

                        break;

                    default:
                        break;
                }
            }
        }
    }
}
