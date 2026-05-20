using Contracts.Interfaces.Lookups.Party;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Command.UpdateMaintenanceRequestCommand;
using FluentValidation;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.MaintenanceRequest
{
    public class UpdateMaintenanceRequestCommandValidator : AbstractValidator<UpdateMaintenanceRequestCommand>
    {
              private readonly List<ValidationRule> _validationRules;
        private readonly IMaintenanceRequestQueryRepository  _maintenanceRequestQueryRepository;
        private readonly ISupplierLookup _supplierLookup;


      public UpdateMaintenanceRequestCommandValidator(IMaintenanceRequestQueryRepository  maintenanceRequestQueryRepository, ISupplierLookup supplierLookup)
      {
         _maintenanceRequestQueryRepository = maintenanceRequestQueryRepository;
         _supplierLookup = supplierLookup;
             // Load validation rules internally — no injection needed
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
                            .WithMessage($"{nameof(UpdateMaintenanceRequestCommand.MaintenanceTypeId)} {rule.Error}");

                        

                        RuleFor(x => x.MachineId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateMaintenanceRequestCommand.MachineId)} {rule.Error}");

                        RuleFor(x => x.RequestTypeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateMaintenanceRequestCommand.RequestTypeId)} {rule.Error}");

                        RuleFor(x => x.MachineId)
                            .GreaterThan(0)
                            .WithMessage("MachineId is required.");

                        RuleFor(x => x.RequestTypeId)
                            .GreaterThan(0)
                            .WithMessage("RequestTypeId is required.");

                        RuleFor(x => x.MaintenanceTypeId)
                            .GreaterThan(0)
                            .WithMessage("MaintenanceTypeId is required.");

                        // SCRUM-1475: block re-routing an Update to a machine that already has a different
                        // Open / InProgress request. excludeRequestId = x.Id so the request's own row doesn't
                        // count against itself when nothing meaningful has changed.
                        RuleFor(x => x.MachineId)
                            .MustAsync(async (cmd, machineId, ct) =>
                                !await _maintenanceRequestQueryRepository
                                    .HasActiveRequestForMachineAsync(machineId, cmd.Id))
                            .WithMessage("A request for this machine is already Open / In Progress. " +
                                         "Please resolve the existing request before reassigning to it.")
                            .When(x => x.MachineId > 0);
                        break;

                        case "WOclosedStatusCheck":
                        RuleFor(x => x.Id)
                            .MustAsync(async (Id, cancellation) =>
                                !await _maintenanceRequestQueryRepository.GetWOclosedOrInProgressAsync(Id))
                             .WithMessage(x => $"{x.Id} {rule.Error}");
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

            // BR-01 / BR-02: selected vendor must be an active supplier in Party Master.
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