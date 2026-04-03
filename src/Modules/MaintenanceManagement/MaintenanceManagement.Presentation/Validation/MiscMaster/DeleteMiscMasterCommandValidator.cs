using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using FluentValidation;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.MiscMaster
{
    public class DeleteMiscMasterCommandValidator : AbstractValidator<DeleteMiscMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;


        public DeleteMiscMasterCommandValidator( IMiscMasterQueryRepository miscMasterQueryRepository)
        {
            
              _validationRules = ValidationRuleLoader.LoadValidationRules();
            _miscMasterQueryRepository = miscMasterQueryRepository;

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
                            .WithMessage($"{nameof(DeleteMiscMasterCommand.Id)} {rule.Error}");
                        break;
                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (Id, cancellation) => !await _miscMasterQueryRepository.SoftDeleteValidationAsync(Id))
                            .WithMessage("This master is linked with other records. You cannot delete this record.");
                        break;
                        case "NotFound":
                           RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _miscMasterQueryRepository.NotFoundAsync(Id))             
                           .WithName("MiscMaster Id")
                            .WithMessage($"{rule.Error}");
                            break; 
                    default:
                        
                        break;
                }
            }

        }

    }
}