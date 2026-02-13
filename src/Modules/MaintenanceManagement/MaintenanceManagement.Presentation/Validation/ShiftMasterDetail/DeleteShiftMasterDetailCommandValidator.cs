using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.IShiftMasterDetail;
using MaintenanceManagement.Application.ShiftMasterDetails.Commands.DeleteShiftMasterDetail;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.ShiftMasterDetail
{
    public class DeleteShiftMasterDetailCommandValidator : AbstractValidator<DeleteShiftMasterDetailCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IShiftMasterDetailQuery _shiftMasterDetailQuery;
        public DeleteShiftMasterDetailCommandValidator( IShiftMasterDetailQuery shiftMasterDetailQuery)
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
                            .WithMessage($"{nameof(DeleteShiftMasterDetailCommand.Id)} {rule.Error}");
                     
                        break;
                      case "NotFound":
                           RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _shiftMasterDetailQuery.NotFoundAsync(Id))             
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