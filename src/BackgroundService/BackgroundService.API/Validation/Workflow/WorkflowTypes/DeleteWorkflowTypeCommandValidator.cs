using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.API.Validation.Common;
using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Commands.DeleteWorkflowType;
using FluentValidation;

namespace BackgroundService.API.Validation.Workflow.WorkflowTypes
{
    public class DeleteWorkflowTypeCommandValidator : AbstractValidator<DeleteWorkflowTypeCommand>
    {
        private readonly List<Common.ValidationRule> _validationRules;
        private readonly IWorkflowTypeQuery _workflowTypeQuery;
        public DeleteWorkflowTypeCommandValidator(IWorkflowTypeQuery workflowTypeQuery)
        {
            _workflowTypeQuery = workflowTypeQuery;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            
             if (_validationRules == null || !_validationRules.Any())
            {
                throw new ArgumentException("Validation rules could not be loaded.");
            }
              foreach (var rule in _validationRules)
            {
                  switch (rule.Rule)
                {
                       case "NotEmpty":
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteWorkflowTypeCommand.Id)} {rule.Error}");
                        break;
                        case "NotFound":
                           RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _workflowTypeQuery.NotFoundAsync(Id))             
                           .WithName("Workflow Type Id")
                            .WithMessage($"{rule.Error}");
                            break; 
                    default:
                        
                        break;
                }
            }
        }
    }
}