using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeeder;
using MaintenanceManagement.Application.Power.Feeder.Command.CreateFeeder;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.Power.Feeder
{
    public class CreateFeederCommandValidator : AbstractValidator<CreateFeederCommand>
    {
      private readonly List<ValidationRule> _validationRules;

        private readonly IFeederQueryRepository _feederQueryRepository;

        public CreateFeederCommandValidator( IFeederQueryRepository feederQueryRepository, MaxLengthProvider maxLengthProvider)
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
                            .WithMessage($"{nameof(CreateFeederCommand.FeederCode)} {rule.Error}");
                        break;
                    case "MaxLength":
                        RuleFor(x => x.FeederCode)
                            .MaximumLength(FeederCodeMaxLength)
                            .WithMessage($"{nameof(CreateFeederCommand.FeederCode)} {rule.Error}");
                        RuleFor(x => x.FeederName)
                            .MaximumLength(FeederNameMaxLength)
                            .WithMessage($"{nameof(CreateFeederCommand.FeederName)} {rule.Error}");
                        break;

                        
                           case "AlreadyExists":
                       RuleFor(x => x.FeederCode)
                        .MustAsync(async (request, feederCode, cancellation) =>
                            feederCode != null && !await _feederQueryRepository.AlreadyExistsAsync(feederCode, request.UnitId))
                        .WithMessage((request, feederCode) =>
                            $"FeederGroupCode '{feederCode}' already exists in Unit ID: {request.UnitId}");
                        break;    

                }
            }
        }
        
    }
}