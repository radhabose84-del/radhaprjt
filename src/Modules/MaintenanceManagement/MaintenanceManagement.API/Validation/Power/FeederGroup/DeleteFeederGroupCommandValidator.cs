using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Command.DeleteFeederGroup;
using FluentValidation;
using MaintenanceManagement.API.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.API.Validation.Power.FeederGroup
{
    public class DeleteFeederGroupCommandValidator : AbstractValidator<DeleteFeederGroupCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IFeederGroupQueryRepository _feederGroupQueryRepository;

        public DeleteFeederGroupCommandValidator(IFeederGroupQueryRepository feederGroupQueryRepository, MaxLengthProvider maxLengthProvider)
        {

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
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteFeederGroupCommand.Id)} {rule.Error}");
                        break;
                        case "NotFound":
                           RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _feederGroupQueryRepository.NotFoundAsync(Id))             
                           .WithName("FeederGroup Id")
                            .WithMessage($"{rule.Error}");
                            break; 

                      case "SoftDelete":
                         RuleFor(x => x.Id)
                      .MustAsync(async (Id, cancellation) => !await _feederGroupQueryRepository.SoftDeleteValidation(Id))
                        .WithMessage($"{rule.Error}");
                        break;
                    default:
                        break;

                }
            }
    
        }
        
    }
}