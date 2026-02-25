using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.MapMachine;
using FluentValidation;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.PreventiveSchedulers
{

    public class MapMachineCommandValidator : AbstractValidator<MapMachineCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
        private readonly IMachineMasterQueryRepository _machineMasterQueryRepository;
        
        public MapMachineCommandValidator(IPreventiveSchedulerQuery preventiveSchedulerQuery,IMachineMasterQueryRepository machineMasterQueryRepository)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _preventiveSchedulerQuery = preventiveSchedulerQuery;
            _machineMasterQueryRepository = machineMasterQueryRepository;

            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                                   .NotNull()
                                   .WithMessage($"{nameof(MapMachineCommand.Id)} {rule.Error}")
                                   .NotEmpty()
                                   .WithMessage($"{nameof(MapMachineCommand.Id)} {rule.Error}");
                        RuleFor(x => x.MachineId)
                                .NotNull()
                                .WithMessage($"{nameof(MapMachineCommand.MachineId)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(MapMachineCommand.MachineId)} {rule.Error}");
                        RuleFor(x => x.LastMaintenanceActivityDate)
                                .NotNull()
                                .WithMessage($"{nameof(MapMachineCommand.LastMaintenanceActivityDate)} {rule.Error}")
                                .NotEmpty()
                                .WithMessage($"{nameof(MapMachineCommand.LastMaintenanceActivityDate)} {rule.Error}");

                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                              .MustAsync(async (Id, cancellation) =>
                           await _preventiveSchedulerQuery.NotFoundAsync(Id))
                               .WithMessage($"{rule.Error}");
                        RuleFor(x => x.MachineId)
                              .MustAsync(async (MachineId, cancellation) =>
                           await _machineMasterQueryRepository.NotFoundAsync(MachineId))
                               .WithMessage($"{rule.Error}");                               
                        break;

                    default:
                        break;
                }
            }

        }
    }
}