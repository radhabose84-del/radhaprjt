using BackgroundService.Application.Workflow.Common.Interfaces.IWorkflowType;
using BackgroundService.Application.Workflow.WorkflowTypes.Commands.CreateWorkflowType;
using FluentValidation;
using Shared.Validation.Common;
using BackgroundService.Presentation.Validation.Common;

namespace BackgroundService.Presentation.Validation.Workflow.WorkflowTypes
{
    public class CreateWorkflowTypeCommandValidator : AbstractValidator<CreateWorkflowTypeCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IWorkflowTypeQuery _workflowTypeQuery;
        public CreateWorkflowTypeCommandValidator(MaxLengthProvider maxLengthProvider, IWorkflowTypeQuery workflowTypeQuery)
        {
            // var maxLength = maxLengthProvider.GetMaxLength<WorkflowType>("ModuleTypeName") ?? 50;
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
                        RuleFor(x => x.MenuId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateWorkflowTypeCommand.MenuId)} {rule.Error}");
                        break;
                    // case "MaxLength":                        
                    //     RuleFor(x => x.MenuId)
                    //         .MaximumLength(maxLength)
                    //         .WithMessage($"{nameof(CreateWorkflowTypeCommand.MenuId)} {rule.Error}");
                        // break;
                    case "AlreadyExists":                       
                        RuleFor(x => new { x.MenuId, x.ModuleId })
                          .MustAsync(async (WorkflowType, cancellation) => !await _workflowTypeQuery.AlreadyExistsAsync(WorkflowType.MenuId,WorkflowType.ModuleId))
                           .WithName("Module Type Name")
                             .WithMessage($"{rule.Error}");
                        break;

                }
            }
        }
    }
}