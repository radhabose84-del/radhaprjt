using MaintenanceManagement.Application.Common.Interfaces.Power.IFeeder;
using MaintenanceManagement.Application.Power.Feeder.Command.UpdateFeeder;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.Power.Feeder
{
    public class UpdateFeederCommandValidator : AbstractValidator<UpdateFeederCommand>
    {
        private readonly List<ValidationRule> _validationRules;

        private readonly IFeederQueryRepository _feederQueryRepository;
          public UpdateFeederCommandValidator( IFeederQueryRepository feederQueryRepository, MaxLengthProvider maxLengthProvider)
        {
            var FeederCodeMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.Power.Feeder>("FeederCode") ?? 50;
            var FeederNameMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.Power.Feeder>("FeederName") ?? 500;
             _validationRules = ValidationRuleLoader.LoadValidationRules();
            _feederQueryRepository = feederQueryRepository;

             if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.FeederCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateFeederCommand.FeederCode)} {rule.Error}");
                        break;
                    case "MaxLength":
                        RuleFor(x => x.FeederCode)
                            .MaximumLength(FeederCodeMaxLength)
                            .WithMessage($"{nameof(UpdateFeederCommand.FeederCode)} {rule.Error}");
                        RuleFor(x => x.FeederName)
                            .MaximumLength(FeederNameMaxLength)
                            .WithMessage($"{nameof(UpdateFeederCommand.FeederName)} {rule.Error}");
                        RuleFor(x => x.FeederTypeId)
                         .NotEmpty()
                         .WithMessage($"{nameof(UpdateFeederCommand.FeederTypeId)} {rule.Error}");

                        break;
                      
                        case "AlreadyExists":
                       RuleFor(x => x.FeederCode)
                        .MustAsync(async (request, feederCode, cancellation) =>
                            feederCode != null && !await _feederQueryRepository.AlreadyExistsAsync(feederCode, request.Id))
                        .WithMessage((request, feederCode) =>
                            $"FeederGroupCode '{feederCode}' already exists in Unit ID: {request.UnitId}");

                        break; 

                }
            }
        }

    }
}