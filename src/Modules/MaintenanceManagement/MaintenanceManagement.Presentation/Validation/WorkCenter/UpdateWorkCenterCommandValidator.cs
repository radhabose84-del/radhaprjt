using MaintenanceManagement.Application.Common.Interfaces.IWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Command.UpdateWorkCenter;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Common;
using Serilog;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.WorkCenter
{
    public class UpdateWorkCenterCommandValidator : AbstractValidator<UpdateWorkCenterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IWorkCenterCommandRepository _iWorkCenterCommandRepository;
        private readonly IWorkCenterQueryRepository _iWorkCenterQueryRepository;
         public UpdateWorkCenterCommandValidator(MaxLengthProvider maxLengthProvider, IWorkCenterCommandRepository iWorkCenterCommandRepository, IWorkCenterQueryRepository iWorkCenterQueryRepository)
        {
            _iWorkCenterCommandRepository=iWorkCenterCommandRepository;
            _iWorkCenterQueryRepository=iWorkCenterQueryRepository;
            var WorkCenterNameMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.WorkCenter>("WorkCenterName") ?? 100;
             // Load validation rules from JSON or another source
              _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
            // Loop through the rules and apply them
            foreach (var rule in _validationRules)
            {
                 switch (rule.Rule)
                {
                    case "NotEmpty":
                        // Apply NotEmpty validation
                        RuleFor(x => x.WorkCenterName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateWorkCenterCommand.WorkCenterName)} {rule.Error}");
                        RuleFor(x => x.UnitId)
                            .NotEmpty()
                            .GreaterThan(0).WithMessage("Unit Id must be greater than 0.");
                        RuleFor(x => x.DepartmentId)
                            .NotEmpty()
                            .GreaterThan(0).WithMessage("Department Id must be greater than 0.");
                        break;
                    case "MaxLength":
                        RuleFor(x => x.WorkCenterName)
                            .MaximumLength(WorkCenterNameMaxLength)
                            .WithMessage($"{nameof(UpdateWorkCenterCommand.WorkCenterName)} {rule.Error} {WorkCenterNameMaxLength}");
                        break;
                    case "AlphaNumericWithPunctuation":
                        RuleFor(x => x.WorkCenterName)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(UpdateWorkCenterCommand.WorkCenterName)} {rule.Error}");
                        break;
                  case "AlreadyExists":
                        RuleFor(x => x.WorkCenterName)
                            .MustAsync(async (x, WorkCenterName, cancellation) => 
                                !await _iWorkCenterCommandRepository.IsNameDuplicateAsync(WorkCenterName, x.Id))
                            .WithName("WorkCenter Name")
                            .WithMessage($"{rule.Error}");
                        break;
                    case "RecordNotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, cancellation) => 
                                (await _iWorkCenterQueryRepository.GetByIdAsync(id)) != null) 
                            .WithName("Id")
                            .WithMessage($"{rule.Error}");
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