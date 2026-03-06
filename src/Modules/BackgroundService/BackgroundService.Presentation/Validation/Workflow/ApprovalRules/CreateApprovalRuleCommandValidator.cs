using BackgroundService.Application.Workflow.ApprovalRules.Commands.CreateApprovalRule;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRule;
using FluentValidation;
using Shared.Validation.Common;
using BackgroundService.Presentation.Validation.Common;

namespace BackgroundService.Presentation.Validation.Workflow.ApprovalRules
{
    public class CreateApprovalRuleCommandValidator : AbstractValidator<CreateApprovalRuleCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IApprovalRuleQuery _approvalRuleQuery;
        public CreateApprovalRuleCommandValidator(MaxLengthProvider maxLengthProvider, IApprovalRuleQuery approvalRuleQuery)
        {
            //   var maxLength = maxLengthProvider.GetMaxLength<ApprovalRule>("ConditionKey") ?? 255;
            //   var Operator = maxLengthProvider.GetMaxLength<ApprovalRule>("Operator") ?? 50;
            //   var Value = maxLengthProvider.GetMaxLength<ApprovalRule>("Value") ?? 50;
            //   var Action = maxLengthProvider.GetMaxLength<ApprovalRule>("Action") ?? 50;

                _approvalRuleQuery = approvalRuleQuery;

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
                        // RuleFor(x => x.ConditionKey)
                        //     .NotEmpty()
                        //     .WithMessage($"{nameof(CreateApprovalRuleCommand.ConditionKey)} {rule.Error}");
                        // RuleFor(x => x.Operator)
                        //     .NotEmpty()
                        //     .WithMessage($"{nameof(CreateApprovalRuleCommand.Operator)} {rule.Error}");
                        // RuleFor(x => x.Value)
                        //     .NotEmpty()
                        //     .WithMessage($"{nameof(CreateApprovalRuleCommand.Value)} {rule.Error}");
                       RuleFor(x => x.ActionId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateApprovalRuleCommand.ActionId)} {rule.Error}");
                       
                        break;
                    case "MaxLength":                        
                        // RuleFor(x => x.ConditionKey)
                        //     .MaximumLength(maxLength)
                        //     .WithMessage($"{nameof(CreateApprovalRuleCommand.ConditionKey)} {rule.Error}");
                        // RuleFor(x => x.Operator)
                        //     .MaximumLength(Operator)
                        //     .WithMessage($"{nameof(CreateApprovalRuleCommand.Operator)} {rule.Error}");
                        // RuleFor(x => x.Value)
                        //     .MaximumLength(Value)
                        //     .WithMessage($"{nameof(CreateApprovalRuleCommand.Value)} {rule.Error}");
                        // RuleFor(x => x.Action)
                        //     .MaximumLength(Action)
                        //     .WithMessage($"{nameof(CreateApprovalRuleCommand.Action)} {rule.Error}");
                        break;
                    case "AlreadyExists":                       
                        // RuleFor(x => new { x.ConditionKey, x.Operator, x.Value, x.Action, x.UnitId, x.WorkflowTypeId })
                        //   .MustAsync(async (approvalType, cancellation) => !await _approvalRuleQuery.AlreadyExistsAsync(approvalType.ConditionKey,approvalType.Operator,approvalType.Value,approvalType.Action,approvalType.UnitId,approvalType.WorkflowTypeId))
                        //    .WithName("Approval Rule")
                        //      .WithMessage($"{rule.Error}");
                        break;

                }
            }
        }
    }
}