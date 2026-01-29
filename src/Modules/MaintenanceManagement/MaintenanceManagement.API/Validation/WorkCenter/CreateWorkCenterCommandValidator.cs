using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.IWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Command.CreateWorkCenter;
using FluentValidation;
using MaintenanceManagement.API.Validation.Common;
using Serilog;
using Shared.Validation.Common;

namespace MaintenanceManagement.API.Validation.WorkCenter
{
    public class CreateWorkCenterCommandValidator : AbstractValidator<CreateWorkCenterCommand> 
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IWorkCenterCommandRepository _iWorkCenterCommandRepository;
        public CreateWorkCenterCommandValidator(MaxLengthProvider maxLengthProvider, IWorkCenterCommandRepository iWorkCenterCommandRepository)
        {
            _iWorkCenterCommandRepository=iWorkCenterCommandRepository;
            var WorkCenterCodeMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.WorkCenter>("WorkCenterCode") ?? 10;
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
                        RuleFor(x => x.WorkCenterCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateWorkCenterCommand.WorkCenterCode)} {rule.Error}");
                        RuleFor(x => x.WorkCenterName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateWorkCenterCommand.WorkCenterName)} {rule.Error}");
                        RuleFor(x => x.UnitId)
                            .NotEmpty()
                            .GreaterThan(0).WithMessage("Unit Id must be greater than 0.");
                        RuleFor(x => x.DepartmentId)
                            .NotEmpty()
                            .GreaterThan(0).WithMessage("Department Id must be greater than 0.");
                        break;
                    case "MaxLength":
                        RuleFor(x => x.WorkCenterCode)
                            .MaximumLength(WorkCenterCodeMaxLength)
                            .WithMessage($"{nameof(CreateWorkCenterCommand.WorkCenterCode)} {rule.Error} {WorkCenterCodeMaxLength}");
                        RuleFor(x => x.WorkCenterName)
                            .MaximumLength(WorkCenterNameMaxLength)
                            .WithMessage($"{nameof(CreateWorkCenterCommand.WorkCenterName)} {rule.Error} {WorkCenterNameMaxLength}");
                        break;
                    
                    case "AlphanumericOnly":
                              RuleFor(x => x.WorkCenterCode)
                             .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern)) 
                             .WithMessage($"{nameof(CreateWorkCenterCommand.WorkCenterCode)} {rule.Error}");   
                        break;
                    case "AlphaNumericWithPunctuation":
                        RuleFor(x => x.WorkCenterName)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(CreateWorkCenterCommand.WorkCenterName)} {rule.Error}");
                        break;

                     case "AlreadyExists":
                            RuleFor(x => x.WorkCenterCode)
                           .MustAsync(async (WorkCenterCode, cancellation) => !await _iWorkCenterCommandRepository.ExistsByCodeAsync(WorkCenterCode))
                           .WithName("WorkCenter Code")
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