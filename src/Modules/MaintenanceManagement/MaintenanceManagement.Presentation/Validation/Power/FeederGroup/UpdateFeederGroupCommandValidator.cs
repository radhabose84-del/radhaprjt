using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Command.UpdateFeederGroup;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.Power.FeederGroup
{
    public class UpdateFeederGroupCommandValidator  : AbstractValidator<UpdateFeederGroupCommand>
    {
    
            private readonly List<ValidationRule> _validationRules;
             private readonly IFeederGroupQueryRepository _feederGroupQueryRepository;

     
           public UpdateFeederGroupCommandValidator(MaxLengthProvider maxLengthProvider ,IFeederGroupQueryRepository feederGroupQueryRepository)
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
                            .WithMessage($"{nameof(UpdateFeederGroupCommand.FeederGroupCode)} {rule.Error}");
                        break;
                    case "MaxLength":
                        RuleFor(x => x.FeederGroupCode)
                            .MaximumLength(FeederGroupCodeMaxLength)
                            .WithMessage($"{nameof(UpdateFeederGroupCommand.FeederGroupCode)} {rule.Error}");
                        RuleFor(x => x.FeederGroupName)
                            .MaximumLength(FeederGroupNameMaxLength)
                            .WithMessage($"{nameof(UpdateFeederGroupCommand.FeederGroupName)} {rule.Error}");
                        break;
                    default:
                        break;

                    case "AlreadyExists":
                       RuleFor(x => x.FeederGroupCode)
                        .MustAsync(async (request, feederGroupCode, cancellation) =>
                            feederGroupCode != null && !await _feederGroupQueryRepository.AlreadyExistsAsync(feederGroupCode, request.Id))
                        .WithMessage((request, feederGroupCode) =>
                            $"FeederGroupCode '{feederGroupCode}' already exists in Unit ID: {request.UnitId}");

                        break;  
                }
            }



        }
           
        
    }
}