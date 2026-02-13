using BudgetManagement.Application.Common.Interfaces.IMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using FluentValidation;
using Shared.Validation.Common;

namespace BudgetManagement.Presentation.Validation.MiscTypeMaster
{
    public class DeleteMiscTypeMasterCommandValidator : AbstractValidator<DeleteMiscTypeMasterCommand>
    {
      
        private readonly List<ValidationRule> _validationRules;
        private readonly IMiscTypeMasterQueryRepository _miscTypeMasterQueryRepository;

        public DeleteMiscTypeMasterCommandValidator( IMiscTypeMasterQueryRepository miscTypeMasterQueryRepository)
        {
             _validationRules = ValidationRuleLoader.LoadValidationRules();
            _miscTypeMasterQueryRepository = miscTypeMasterQueryRepository;
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
                            .WithMessage($"{nameof(DeleteMiscTypeMasterCommand.Id)} {rule.Error}");
                        break;
                    case "SoftDelete":
                         RuleFor(x => x.Id)
                      .MustAsync(async (Id, cancellation) => !await _miscTypeMasterQueryRepository.SoftDeleteValidation(Id))
                        .WithMessage($"{rule.Error}");
                        break;
                        case "NotFound":
                           RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _miscTypeMasterQueryRepository.NotFoundAsync(Id))             
                           .WithName("MiscType Id")
                            .WithMessage($"{rule.Error}");
                            break; 
                    default:
                        
                        break;
                }
            }
        } 
        
    }
}