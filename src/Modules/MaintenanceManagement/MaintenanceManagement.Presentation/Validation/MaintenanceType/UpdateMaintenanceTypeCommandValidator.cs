using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Command.UpdateMaintenanceType;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Common;
using Serilog;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.MaintenanceType
{
    public class UpdateMaintenanceTypeCommandValidator : AbstractValidator<UpdateMaintenanceTypeCommand> 
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMaintenanceTypeCommandRepository _imaintenanceTypeCommandRepository;
        private readonly IMaintenanceTypeQueryRepository _imaintenanceTypeQueryRepository;


        public UpdateMaintenanceTypeCommandValidator(IMaintenanceTypeCommandRepository imaintenanceTypeCommandRepository,MaxLengthProvider maxLengthProvider,IMaintenanceTypeQueryRepository imaintenanceTypeQueryRepository) 
        {
            _imaintenanceTypeCommandRepository = imaintenanceTypeCommandRepository;
            _imaintenanceTypeQueryRepository=imaintenanceTypeQueryRepository;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            var MaintenanceTypeMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.MaintenanceType>("TypeName") ?? 100;
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.TypeName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateMaintenanceTypeCommand.TypeName)} {rule.Error}");
                        break;
                    case "AlphaNumericWithPunctuation":
                        RuleFor(x => x.TypeName)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(UpdateMaintenanceTypeCommand.TypeName)} {rule.Error}");
                        break;
                     case "AlreadyExists":
                        RuleFor(x => x.TypeName)
                            .MustAsync(async (x, TypeName, cancellation) => 
                                !await _imaintenanceTypeCommandRepository.IsNameDuplicateAsync(TypeName, x.Id))
                            .WithName("MachineType Name")
                            .WithMessage($"{rule.Error}");
                        break;
                      case "RecordNotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, cancellation) => 
                                (await _imaintenanceTypeQueryRepository.GetByIdAsync(id)) != null) 
                            .WithName("Id")
                            .WithMessage($"{rule.Error}");
                            break;
                         case "MaxLength":
                        RuleFor(x => x.TypeName)
                            .MaximumLength(MaintenanceTypeMaxLength)
                            .WithMessage($"{nameof(UpdateMaintenanceTypeCommand.TypeName)} {rule.Error} {MaintenanceTypeMaxLength}");
                            break;
                    default:
                        // Handle unknown rule (log or throw)
                        Log.Information("Warning: Unknown rule '{Rule}' encountered.", rule.Rule);
                        break;
                }
            }
        }
    }
}