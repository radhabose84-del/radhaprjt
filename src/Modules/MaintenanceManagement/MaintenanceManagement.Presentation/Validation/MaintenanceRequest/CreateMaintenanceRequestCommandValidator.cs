using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Command.CreateMaintenanceRequest;
using FluentValidation;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.MaintenanceRequest
{
    public class CreateMaintenanceRequestCommandValidator : AbstractValidator<CreateMaintenanceRequestCommand>
    {
      private readonly List<ValidationRule> _validationRules;  
        private readonly IMaintenanceRequestQueryRepository  _maintenanceRequestQueryRepository;
        


      public CreateMaintenanceRequestCommandValidator(  IMaintenanceRequestQueryRepository  maintenanceRequestQueryRepository)
      {
         _maintenanceRequestQueryRepository = maintenanceRequestQueryRepository;
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

                        // SCRUM-1475: block creating a request when the machine already has an Open / InProgress one
                        RuleFor(x => x.MachineId)
                            .MustAsync(async (machineId, ct) =>
                                !await _maintenanceRequestQueryRepository.HasActiveRequestForMachineAsync(machineId))
                            .WithMessage("A request for this machine is already Open / In Progress. " +
                                         "Please resolve the existing request before creating a new one.")
                            .When(x => x.MachineId > 0);

                        break;

                    default:
                        break;
                }
            }
      }

      



        
    }
}