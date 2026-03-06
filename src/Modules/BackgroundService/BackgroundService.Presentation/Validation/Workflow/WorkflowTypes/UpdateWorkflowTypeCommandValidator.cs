using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Commands.UpdateWorkflowType;
using FluentValidation;
using Shared.Validation.Common;

namespace BackgroundService.Presentation.Validation.Workflow.WorkflowTypes
{
    public class UpdateWorkflowTypeCommandValidator : AbstractValidator<UpdateWorkflowTypeCommand>
    {
         private readonly List<ValidationRule> _validationRules;
        private readonly IWorkflowTypeQuery _workflowTypeQuery;
        public UpdateWorkflowTypeCommandValidator(IWorkflowTypeQuery workflowTypeQuery)
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
                        RuleFor(x => x.ModuleId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateWorkflowTypeCommand.ModuleId)} {rule.Error}");

                            RuleFor(x => x.MenuId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateWorkflowTypeCommand.MenuId)} {rule.Error}");
                        break;
                    case "AlreadyExists":
                        RuleFor(x => new { x.MenuId,x.ModuleId, x.Id })
                         .MustAsync(async (WorkflowType, cancellation) =>
                      !await _workflowTypeQuery.AlreadyExistsAsync(WorkflowType.MenuId,WorkflowType.ModuleId, WorkflowType.Id))
                         .WithName("Module Type Name")
                          .WithMessage($"{rule.Error}");
                        break;
                        
                    case "NotFound":
                           RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _workflowTypeQuery.NotFoundAsync(Id))             
                           .WithName("Workflow Type Id")
                            .WithMessage($"{rule.Error}");
                            break; 

                }
            }
        }
    }
}