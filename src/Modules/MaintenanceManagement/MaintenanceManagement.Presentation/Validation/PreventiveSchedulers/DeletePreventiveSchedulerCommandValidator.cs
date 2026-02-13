using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.DeletePreventiveScheduler;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.PreventiveSchedulers
{
    public class DeletePreventiveSchedulerCommandValidator : AbstractValidator<DeletePreventiveSchedulerCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IPreventiveSchedulerQuery _preventiveSchedulerQuery;
        
        public DeletePreventiveSchedulerCommandValidator(IPreventiveSchedulerQuery preventiveSchedulerQuery)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _preventiveSchedulerQuery = preventiveSchedulerQuery;
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
                                .WithMessage($"{nameof(DeletePreventiveSchedulerCommand.Id)} {rule.Error}");
                    break;     
                    case "NotFound":
                     RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _preventiveSchedulerQuery.NotFoundAsync(Id))
                            .WithMessage($"{rule.Error}");
                            break;  
                    case "PreventiveDelete":
                     RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _preventiveSchedulerQuery.UpdateValidation(Id))
                            .WithMessage($"{rule.Error}");
                            break;
                    default:                        
                        break;                            
                }
            }
        }
    }
}