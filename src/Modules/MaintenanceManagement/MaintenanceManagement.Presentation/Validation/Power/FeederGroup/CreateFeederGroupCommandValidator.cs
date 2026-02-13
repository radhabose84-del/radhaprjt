using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Command.CreateFeederGroup;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.Power.FeederGroup
{
    public class CreateFeederGroupCommandValidator  : AbstractValidator<CreateFeederGroupCommand>
    {

        private readonly List<ValidationRule> _validationRules;
        


        private readonly IFeederGroupQueryRepository _feederGroupQueryRepository;

        public CreateFeederGroupCommandValidator(IFeederGroupQueryRepository feederGroupQueryRepository, MaxLengthProvider maxLengthProvider , IIPAddressService ipAddressService)
        {
            var FeederGroupCodeMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.Power.FeederGroup>("FeederGroupCode") ?? 50;
            var FeederGroupNameMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.Power.FeederGroup>("FeederGroupName") ?? 250;
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _feederGroupQueryRepository = feederGroupQueryRepository;
          
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
             foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.FeederGroupCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateFeederGroupCommand.FeederGroupCode)} {rule.Error}");
                        break;
                    case "MaxLength":
                        RuleFor(x => x.FeederGroupCode)
                            .MaximumLength(FeederGroupCodeMaxLength)
                            .WithMessage($"{nameof(CreateFeederGroupCommand.FeederGroupCode)} {rule.Error}");
                        RuleFor(x => x.FeederGroupName)
                            .MaximumLength(FeederGroupNameMaxLength)
                            .WithMessage($"{nameof(CreateFeederGroupCommand.FeederGroupName)} {rule.Error}");
                        break;
                    default:
                        break;

                    case "AlreadyExists":
                       RuleFor(x => x.FeederGroupCode)
                        .MustAsync(async (request, feederGroupCode, cancellation) =>
                            feederGroupCode != null && !await _feederGroupQueryRepository.AlreadyExistsAsync(feederGroupCode, request.UnitId))
                        .WithMessage((request, feederGroupCode) =>
                            $"FeederGroupCode '{feederGroupCode}' already exists in Unit ID: {request.UnitId}");
                        break;      
                }
            }



        }
           



    }
}