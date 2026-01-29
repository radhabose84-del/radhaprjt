using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMaster;
using MaintenanceManagement.Application.ShiftMasters.Commands.CreateShiftMaster;
using FluentValidation;
using MaintenanceManagement.API.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.API.Validation.ShiftMaster
{
    public class CreateShiftMasterCommandValidator : AbstractValidator<CreateShiftMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IShiftMasterQuery _shiftMasterQuery;
        public CreateShiftMasterCommandValidator(MaxLengthProvider maxLengthProvider,IShiftMasterQuery shiftMasterQuery)
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
                        RuleFor(x => x.ShiftCode)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateShiftMasterCommand.ShiftCode)} {rule.Error}");
                        RuleFor(x => x.ShiftName)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateShiftMasterCommand.ShiftName)} {rule.Error}");
                        RuleFor(x => x.EffectiveDate)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateShiftMasterCommand.EffectiveDate)} {rule.Error}");
                        break;
                     case "MaxLength":
                    
                        RuleFor(x => x.ShiftCode)
                            .MaximumLength(ShiftCodeMaxLength)
                            .WithMessage($"{nameof(CreateShiftMasterCommand.ShiftCode)} {rule.Error}{ShiftCodeMaxLength}");
                        RuleFor(x => x.ShiftName)
                            .MaximumLength(ShiftNameMaxLength)
                            .WithMessage($"{nameof(CreateShiftMasterCommand.ShiftName)} {rule.Error}{ShiftNameMaxLength}");
                        break;
                    case "AlreadyExists":
                           RuleFor(x => x.ShiftName)
                           .MustAsync(async (ShiftName, cancellation) => !await _shiftMasterQuery.AlreadyExistsAsync(ShiftName))
                           .WithName("Shift Name")
                            .WithMessage($"{rule.Error}");

                             RuleFor(x => x.ShiftCode)
                           .MustAsync(async (ShiftCode, cancellation) => !await _shiftMasterQuery.AlreadyExistsShiftCodeAsync(ShiftCode))
                           .WithName("Shift Code")
                            .WithMessage($"{rule.Error}");
                            break;
                    
                      default:
                        
                        break;
                }
            }
        }
       
    }
}