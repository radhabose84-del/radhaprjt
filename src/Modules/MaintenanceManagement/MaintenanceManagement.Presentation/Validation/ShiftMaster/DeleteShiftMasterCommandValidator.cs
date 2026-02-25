using MaintenanceManagement.Application.Common.Interfaces.IShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Commands.DeleteShiftMaster;
using FluentValidation;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.ShiftMaster
{
    public class DeleteShiftMasterCommandValidator : AbstractValidator<DeleteShiftMasterCommand>
    {
       private readonly List<ValidationRule> _validationRules;
        private readonly IShiftMasterQuery _shiftMasterQuery; 
        public DeleteShiftMasterCommandValidator(IShiftMasterQuery shiftMasterQuery)
        {
             _validationRules = ValidationRuleLoader.LoadValidationRules();
            _shiftMasterQuery =shiftMasterQuery;
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
                            .WithMessage($"{nameof(DeleteShiftMasterCommand.Id)} {rule.Error}");
                        break;
                    case "SoftDelete":
                         RuleFor(x => x.Id)
                      .MustAsync(async (Id, cancellation) => !await _shiftMasterQuery.SoftDeleteValidation(Id))
                        .WithMessage($"{rule.Error}");
                        break;
                        case "NotFound":
                           RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _shiftMasterQuery.NotFoundAsync(Id))             
                           .WithName("Shift Id")
                            .WithMessage($"{rule.Error}");
                            break; 
                    default:
                        
                        break;
                }
            }
        }
    }
}