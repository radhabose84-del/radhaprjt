using MaintenanceManagement.Application.Power.GeneratorConsumption.Command;
using FluentValidation;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.Power.GeneratorConsumption
{
    public class CreateGeneratorConsumptionCommandValidator : AbstractValidator<CreateGeneratorConsumptionCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        public CreateGeneratorConsumptionCommandValidator()
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
                        RuleFor(x => x.GeneratorId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateGeneratorConsumptionCommand.GeneratorId)} {rule.Error}");
                        RuleFor(x => x.StartTime)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateGeneratorConsumptionCommand.StartTime)} {rule.Error}");
                        RuleFor(x => x.EndTime)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateGeneratorConsumptionCommand.EndTime)} {rule.Error}");
                        RuleFor(x => x.DieselConsumption)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateGeneratorConsumptionCommand.DieselConsumption)} {rule.Error}");
                        RuleFor(x => x.OpeningEnergyReading)
                            .NotEmpty()
                            .GreaterThan(0)
                            .WithMessage("Opening Reading must be a positive number.");
                        RuleFor(x => x.ClosingEnergyReading)
                            .NotEmpty()
                            .GreaterThan(0)
                            .WithMessage("Closing Reading must be a positive number.")
                            .GreaterThan(x => x.OpeningEnergyReading)
                            .WithMessage("Closing Reading must be greater than Opening Reading.");
                        RuleFor(x => x.EndTime)
                            .GreaterThan(x => x.StartTime)
                            .WithMessage("EndTime must be greater than StartTime.");
                        RuleFor(x => x.PurposeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateGeneratorConsumptionCommand.PurposeId)} {rule.Error}");
                        break;
                                                

                    case "MinLength":
                        RuleFor(x => x.GeneratorId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateGeneratorConsumptionCommand.GeneratorId)} {rule.Error} {0}");   
                        RuleFor(x => x.DieselConsumption)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateGeneratorConsumptionCommand.DieselConsumption)} {rule.Error} {0}");   
                        RuleFor(x => x.OpeningEnergyReading)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateGeneratorConsumptionCommand.OpeningEnergyReading)} {rule.Error} {0}");   
                         RuleFor(x => x.ClosingEnergyReading)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateGeneratorConsumptionCommand.ClosingEnergyReading)} {rule.Error} {0}"); 
                        RuleFor(x => x.PurposeId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateGeneratorConsumptionCommand.PurposeId)} {rule.Error} {0}");
                        break;
                    default:
                        break;
                }
            }
        }
    }
}