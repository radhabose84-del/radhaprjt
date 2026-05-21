using Contracts.Interfaces.Lookups.Party;
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
        private readonly ISupplierLookup _supplierLookup;

        public CreateMaintenanceRequestCommandValidator(
            IMaintenanceRequestQueryRepository maintenanceRequestQueryRepository,
            IDepartmentLookup departmentLookup,
            ISupplierLookup supplierLookup)
        {
            _maintenanceRequestQueryRepository = maintenanceRequestQueryRepository;
            _departmentLookup = departmentLookup;
            _supplierLookup = supplierLookup;

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

                        // SCRUM-1475: block creating a new request for a machine that already has an
                        // Open / InProgress request. excludeRequestId is null on Create (no own row yet).
                        RuleFor(x => x.MachineId)
                            .MustAsync(async (machineId, ct) =>
                                !await _maintenanceRequestQueryRepository
                                    .HasActiveRequestForMachineAsync(machineId, null))
                            .WithMessage("A request for this machine is already Open / In Progress. " +
                                         "Please resolve the existing request before creating a new one.")
                            .When(x => x.MachineId > 0);

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

            // ── Vendor selection (External Service Request only) ──────────────
            // BR-03: vendor is mandatory for External requests.
            RuleFor(x => x.VendorId)
                .MustAsync(async (cmd, vendorId, ct) =>
                    !await IsExternalRequestAsync(cmd.RequestTypeId)
                    || (vendorId.HasValue && vendorId.Value > 0))
                .WithMessage("Please select a vendor.");

            // BR-01 / BR-02: the selected vendor must be an active supplier in the
            // ERP Party Master. Only checked when External + a vendor was supplied
            // (the "required" message above covers the missing-vendor case).
            RuleFor(x => x.VendorId)
                .MustAsync(async (cmd, vendorId, ct) =>
                {
                    if (!await IsExternalRequestAsync(cmd.RequestTypeId))
                        return true;
                    if (!vendorId.HasValue || vendorId.Value <= 0)
                        return true;
                    var supplier = await _supplierLookup.GetActiveSupplierByIdAsync(vendorId.Value, ct);
                    return supplier != null;
                })
                .WithMessage("Selected vendor is not a valid active supplier.");
        }

        private async Task<bool> IsExternalRequestAsync(int requestTypeId)
        {
            var externalTypes = await _maintenanceRequestQueryRepository.GetMaintenanceExternalRequestTypeAsync();
            var externalTypeId = externalTypes?.FirstOrDefault()?.Id;
            return externalTypeId.HasValue && requestTypeId == externalTypeId.Value;
        }
    }
}
