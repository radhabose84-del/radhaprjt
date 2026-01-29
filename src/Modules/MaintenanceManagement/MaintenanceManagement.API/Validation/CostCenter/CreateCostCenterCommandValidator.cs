using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using MaintenanceManagement.Application.Common.Interfaces.ICostCenter;
using MaintenanceManagement.Application.CostCenter.Command.CreateCostCenter;
using FluentValidation;
using MaintenanceManagement.API.Validation.Common;
using Serilog;
using Shared.Validation.Common;

namespace MaintenanceManagement.API.Validation.CostCenter
{
    public class CreateCostCenterCommandValidator : AbstractValidator<CreateCostCenterCommand> 
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICostCenterCommandRepository _iCostCenterCommandRepository;
        public CreateCostCenterCommandValidator(MaxLengthProvider maxLengthProvider, ICostCenterCommandRepository iCostCenterCommandRepository)
        {
            _iCostCenterCommandRepository=iCostCenterCommandRepository;
            var CostCenterCodeMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.CostCenter>("CostCenterCode") ?? 10;
            var CostCenterNameMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.CostCenter>("CostCenterName") ?? 100;
            var ResponsiblePersonMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.CostCenter>("ResponsiblePerson") ?? 200;
            var RemarksPersonMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.CostCenter>("Remarks") ?? 250;
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
                        RuleFor(x => x.CostCenterCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCostCenterCommand.CostCenterCode)} {rule.Error}");
                        RuleFor(x => x.CostCenterName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCostCenterCommand.CostCenterName)} {rule.Error}");
                        RuleFor(x => x.UnitId)
                            .NotEmpty()
                            .GreaterThan(0).WithMessage("Unit Id must be greater than 0.");
                        RuleFor(x => x.DepartmentId)
                            .NotEmpty()
                            .GreaterThan(0).WithMessage("Department Id must be greater than 0.");
                        RuleFor(x => x.EffectiveDate)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCostCenterCommand.EffectiveDate)} {rule.Error}")
                            .Must(date => date.Date <= DateTime.UtcNow.Date)
                            .WithMessage("Document Date cannot be in the future.")
                            .Must(date => date.Date >= DateTime.UtcNow.AddYears(-1).Date)
                            .WithMessage("Document Date cannot be older than 1 year.");
                        RuleFor(x => x.ResponsiblePerson)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateCostCenterCommand.ResponsiblePerson)} {rule.Error}");
                       RuleFor(x => x.BudgetAllocated)
                            .Must(value => value == null || value == 0 || value > 0)
                            .WithMessage("Budget Allocation must be a positive number if provided.");
                        break;
                    case "MaxLength":
                        RuleFor(x => x.CostCenterCode)
                            .MaximumLength(CostCenterCodeMaxLength)
                            .WithMessage($"{nameof(CreateCostCenterCommand.CostCenterCode)} {rule.Error} {CostCenterCodeMaxLength}");
                        RuleFor(x => x.CostCenterName)
                            .MaximumLength(CostCenterNameMaxLength)
                            .WithMessage($"{nameof(CreateCostCenterCommand.CostCenterName)} {rule.Error} {CostCenterNameMaxLength}");
                        RuleFor(x => x.ResponsiblePerson)
                            .MaximumLength(ResponsiblePersonMaxLength)
                            .WithMessage($"{nameof(CreateCostCenterCommand.ResponsiblePerson)} {rule.Error} {ResponsiblePersonMaxLength}");
                        RuleFor(x => x.Remarks)
                            .MaximumLength(RemarksPersonMaxLength)
                            .WithMessage($"{nameof(CreateCostCenterCommand.Remarks)} {rule.Error} {RemarksPersonMaxLength}");
                        break;
                    
                    case "AlphanumericOnly":
                              RuleFor(x => x.CostCenterCode)
                             .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern)) 
                             .WithMessage($"{nameof(CreateCostCenterCommand.CostCenterCode)} {rule.Error}");   
                        break;
                    case "AlphaNumericWithPunctuation":
                        RuleFor(x => x.CostCenterName)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(CreateCostCenterCommand.CostCenterName)} {rule.Error}");
                        break;

                    //  case "AlreadyExists":
                    //         RuleFor(x => x.CostCenterCode)
                    //        .MustAsync(async (CostCenterCode, cancellation) => !await _iCostCenterCommandRepository.ExistsByCodeAsync(CostCenterCode))
                    //        .WithName("CostCenter Code")
                    //        .WithMessage($"{rule.Error}");
                    //         break; 
                    case "AlreadyExists":
                            RuleFor(x => new { x.CostCenterCode, x.CostCenterName, x.UnitId })
                                .MustAsync(async (x, cancellation) =>
                                    !await _iCostCenterCommandRepository
                                        .ExistsByCodeOrNameAndUnitAsync(x.CostCenterCode ?? string.Empty , x.CostCenterName?? string.Empty, x.UnitId))
                                .WithName("CostCenter")
                                .WithMessage("CostCenterCode or CostCenterName already exists in this Unit.");
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