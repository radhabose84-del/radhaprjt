using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.ShiftMasterDetails.Commands.CreateShiftMasterDetail;
using FluentValidation;
using MaintenanceManagement.API.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.API.Validation.ShiftMasterDetail
{
    public class CreateShiftMasterDetailCommandValidator : AbstractValidator<CreateShiftMasterDetailCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        public CreateShiftMasterDetailCommandValidator()
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
                        RuleFor(x => x.ShiftMasterId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateShiftMasterDetailCommand.ShiftMasterId)} {rule.Error}");
                        RuleFor(x => x.UnitId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateShiftMasterDetailCommand.UnitId)} {rule.Error}");
                        RuleFor(x => x.StartTime)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateShiftMasterDetailCommand.StartTime)} {rule.Error}");
                        RuleFor(x => x.EndTime)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateShiftMasterDetailCommand.EndTime)} {rule.Error}");
                        RuleFor(x => x.BreakDurationInMinutes)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateShiftMasterDetailCommand.BreakDurationInMinutes)} {rule.Error}");
                        RuleFor(x => x.EffectiveDate)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateShiftMasterDetailCommand.EffectiveDate)} {rule.Error}");
                        RuleFor(x => x.ShiftSupervisorId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateShiftMasterDetailCommand.ShiftSupervisorId)} {rule.Error}");
                        break;
                      case "MinLength":
                        RuleFor(x => x.ShiftMasterId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateShiftMasterDetailCommand.ShiftMasterId)} {rule.Error} {0}");   
                        RuleFor(x => x.UnitId)
                            .GreaterThanOrEqualTo(1)
                            .WithMessage($"{nameof(CreateShiftMasterDetailCommand.UnitId)} {rule.Error} {0}");   
                        break;
                    case "PastDateValidation":
                        RuleFor(x => x.EffectiveDate)
                            .Must(BeTodayOrFuture)
                            .WithMessage($"{nameof(CreateShiftMasterDetailCommand.EffectiveDate)} {rule.Error} {0}");  
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