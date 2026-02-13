using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.ICostCenter;
using MaintenanceManagement.Application.CostCenter.Command.UpdateCostCenter;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Common;
using Serilog;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.CostCenter
{
    public class UpdateCostCenterCommandValidator  : AbstractValidator<UpdateCostCenterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ICostCenterCommandRepository _iCostCenterCommandRepository;
        private readonly ICostCenterQueryRepository _iCostCenterQueryRepository;
        public UpdateCostCenterCommandValidator(MaxLengthProvider maxLengthProvider, ICostCenterCommandRepository iCostCenterCommandRepository, ICostCenterQueryRepository iCostCenterQueryRepository)
        {
            _iCostCenterCommandRepository=iCostCenterCommandRepository;
            _iCostCenterQueryRepository=iCostCenterQueryRepository;
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
                        RuleFor(x => x.CostCenterName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateCostCenterCommand.CostCenterName)} {rule.Error}");
                        RuleFor(x => x.UnitId)
                            .NotEmpty()
                            .GreaterThan(0).WithMessage("Unit Id must be greater than 0.");
                        RuleFor(x => x.DepartmentId)
                            .NotEmpty()
                            .GreaterThan(0).WithMessage("Department Id must be greater than 0.");
                        RuleFor(x => x.EffectiveDate)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateCostCenterCommand.EffectiveDate)} {rule.Error}")
                            .Must(date => date.Date <= DateTime.UtcNow.Date)
                            .WithMessage("Document Date cannot be in the future.")
                            .Must(date => date.Date >= DateTime.UtcNow.AddYears(-1).Date)
                            .WithMessage("Document Date cannot be older than 1 year.");
                        RuleFor(x => x.ResponsiblePerson)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateCostCenterCommand.ResponsiblePerson)} {rule.Error}");
                        RuleFor(x => x.BudgetAllocated)
                            .Must(value => value == null || value == 0 || value > 0)
                            .WithMessage("Budget Allocation must be a positive number if provided.");
                        break;
                    case "MaxLength":
                        RuleFor(x => x.CostCenterName)
                            .MaximumLength(CostCenterNameMaxLength)
                            .WithMessage($"{nameof(UpdateCostCenterCommand.CostCenterName)} {rule.Error} {CostCenterNameMaxLength}");
                        RuleFor(x => x.ResponsiblePerson)
                            .MaximumLength(ResponsiblePersonMaxLength)
                            .WithMessage($"{nameof(UpdateCostCenterCommand.ResponsiblePerson)} {rule.Error} {ResponsiblePersonMaxLength}");
                        RuleFor(x => x.Remarks)
                            .MaximumLength(RemarksPersonMaxLength)
                            .WithMessage($"{nameof(UpdateCostCenterCommand.Remarks)} {rule.Error} {RemarksPersonMaxLength}");
                        break;
                    case "AlphaNumericWithPunctuation":
                        RuleFor(x => x.CostCenterName)
                            .Matches(new System.Text.RegularExpressions.Regex(rule.Pattern))
                            .WithMessage($"{nameof(UpdateCostCenterCommand.CostCenterName)} {rule.Error}");
                        break;
                case "AlreadyExists":
                    RuleFor(x => x.CostCenterName)
                        .MustAsync(async (model, costCenterName, cancellation) =>
                            !await _iCostCenterCommandRepository.IsNameDuplicateAsync(
                                costCenterName, model.Id, model.UnitId))
                        .WithName("CostCenter Name")
                        .WithMessage("CostCenter Name already exists in this Unit.");
                    break;
                    case "RecordNotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, cancellation) => 
                                (await _iCostCenterQueryRepository.GetByIdAsync(id)) != null) 
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