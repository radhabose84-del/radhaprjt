using MaintenanceManagement.Application.Common.Interfaces.IShiftMasterDetail;
using MaintenanceManagement.Application.ShiftMasterDetails.Commands.UpdateShiftMasterDetail;
using FluentValidation;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.ShiftMasterDetail
{
    public class UpdateShiftMasterDetailCommandValidator : AbstractValidator<UpdateShiftMasterDetailCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IShiftMasterDetailQuery _shiftMasterDetailQuery;
        public UpdateShiftMasterDetailCommandValidator( IShiftMasterDetailQuery shiftMasterDetailQuery)
        {
             _validationRules = ValidationRuleLoader.LoadValidationRules();
             _shiftMasterDetailQuery = shiftMasterDetailQuery;
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
                            .WithMessage($"{nameof(UpdateShiftMasterDetailCommand.Id)} {rule.Error}");
                        RuleFor(x => x.ShiftMasterId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateShiftMasterDetailCommand.ShiftMasterId)} {rule.Error}");
                        RuleFor(x => x.UnitId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateShiftMasterDetailCommand.UnitId)} {rule.Error}");
                        RuleFor(x => x.StartTime)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateShiftMasterDetailCommand.StartTime)} {rule.Error}");
                        RuleFor(x => x.EndTime)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateShiftMasterDetailCommand.EndTime)} {rule.Error}");
                        RuleFor(x => x.BreakDurationInMinutes)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateShiftMasterDetailCommand.BreakDurationInMinutes)} {rule.Error}");
                        RuleFor(x => x.EffectiveDate)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateShiftMasterDetailCommand.EffectiveDate)} {rule.Error}");
                        RuleFor(x => x.ShiftSupervisorId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateShiftMasterDetailCommand.ShiftSupervisorId)} {rule.Error}");
                        break;
                      case "MinLength":
                        RuleFor(x => x.ShiftMasterId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(UpdateShiftMasterDetailCommand.ShiftMasterId)} {rule.Error} {0}");   
                        RuleFor(x => x.UnitId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(UpdateShiftMasterDetailCommand.UnitId)} {rule.Error} {0}");   
                        break;
                    case "NotFound":
                           RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _shiftMasterDetailQuery.NotFoundAsync(Id))             
                           .WithName("Shift Id")
                            .WithMessage($"{rule.Error}");
                            break;
                   case "FKColumnActiveOrInactive":
                           RuleFor(x => x.ShiftMasterId )
                           .MustAsync(async (ShiftMasterId, cancellation) => 
                        await _shiftMasterDetailQuery.FKColumnValidation(ShiftMasterId)) 
                            .WithMessage($"{rule.Error}");
                            break; 
                       case "PastDateValidation":
                        RuleFor(x => x.EffectiveDate)
                            .Must(BeTodayOrFuture)
                            .WithMessage($"{nameof(UpdateShiftMasterDetailCommand.EffectiveDate)} {rule.Error} {0}");  
                        break;
                      default:
                        
                        break;
                }
            }
        }
         private bool BeTodayOrFuture(DateOnly date)
          {
              return date.ToDateTime(TimeOnly.MinValue) >= DateTime.UtcNow.Date;
          }
    }
}