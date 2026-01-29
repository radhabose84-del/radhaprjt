using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Commands.UpdateShiftMaster;
using FluentValidation;
using MaintenanceManagement.API.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.API.Validation.ShiftMaster
{
    public class UpdateShiftMasterCommandValidator : AbstractValidator<UpdateShiftMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IShiftMasterQuery _shiftMasterQuery;
        public UpdateShiftMasterCommandValidator(MaxLengthProvider maxLengthProvider,IShiftMasterQuery shiftMasterQuery)
        {
             var ShiftNameMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.ShiftMaster>("ShiftName") ?? 50;
            var ShiftCodeMaxLength = maxLengthProvider.GetMaxLength<MaintenanceManagement.Domain.Entities.ShiftMaster>("ShiftCode") ?? 10;

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
                            .WithMessage($"{nameof(UpdateShiftMasterCommand.Id)} {rule.Error}");
                        RuleFor(x => x.ShiftCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateShiftMasterCommand.ShiftCode)} {rule.Error}");
                        RuleFor(x => x.ShiftName)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateShiftMasterCommand.ShiftName)} {rule.Error}");
                        RuleFor(x => x.EffectiveDate)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateShiftMasterCommand.EffectiveDate)} {rule.Error}");
                        break;
                     case "MaxLength":
                    
                        RuleFor(x => x.ShiftCode)
                            .MaximumLength(ShiftCodeMaxLength)
                            .WithMessage($"{nameof(UpdateShiftMasterCommand.ShiftCode)} {rule.Error}");
                        RuleFor(x => x.ShiftName)
                            .MaximumLength(ShiftNameMaxLength)
                            .WithMessage($"{nameof(UpdateShiftMasterCommand.ShiftName)} {rule.Error}");
                        break;
                    case "AlreadyExists":
                           RuleFor(x =>  new { x.ShiftName, x.Id })
                           .MustAsync(async (shift, cancellation) => 
                        !await _shiftMasterQuery.AlreadyExistsAsync(shift.ShiftName, shift.Id))             
                           .WithName("Shift Name")
                            .WithMessage($"{rule.Error}");


                            RuleFor(x =>  new { x.ShiftCode, x.Id })
                           .MustAsync(async (shift, cancellation) => 
                        !await _shiftMasterQuery.AlreadyExistsShiftCodeAsync(shift.ShiftCode, shift.Id))             
                           .WithName("Shift Code")
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