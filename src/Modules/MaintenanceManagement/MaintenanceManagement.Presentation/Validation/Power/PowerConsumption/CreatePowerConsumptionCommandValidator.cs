using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Power.PowerConsumption.Command.CreatePowerConsumption;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.Power.PowerConsumption
{
    public class CreatePowerConsumptionCommandValidator : AbstractValidator<CreatePowerConsumptionCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        public CreatePowerConsumptionCommandValidator(MaxLengthProvider maxLengthProvider)
        {
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
                        RuleFor(x => x.FeederTypeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreatePowerConsumptionCommand.FeederTypeId)} {rule.Error}");
                        RuleFor(x => x.FeederId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreatePowerConsumptionCommand.FeederId)} {rule.Error}");
                        RuleFor(x => x.OpeningReading)
                            .NotEmpty()
                            .GreaterThan(0)
                            .WithMessage("Opening Reading must be a positive number.");
                        RuleFor(x => x.ClosingReading)
                            .NotEmpty()
                            .GreaterThan(0)
                            .WithMessage("Closing Reading must be a positive number.")
                            .GreaterThan(x => x.OpeningReading)
                            .WithMessage("Closing Reading must be greater than Opening Reading.");
                        break;
                                                

                    case "MinLength":
                        RuleFor(x => x.FeederTypeId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreatePowerConsumptionCommand.FeederTypeId)} {rule.Error} {0}");   
                        RuleFor(x => x.FeederId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreatePowerConsumptionCommand.FeederId)} {rule.Error} {0}");   
                        RuleFor(x => x.OpeningReading)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreatePowerConsumptionCommand.OpeningReading)} {rule.Error} {0}");   
                         RuleFor(x => x.ClosingReading)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreatePowerConsumptionCommand.ClosingReading)} {rule.Error} {0}"); 
                        
                        break;
                   
                    default:
                        break;
                }
            }

        }
    }
}