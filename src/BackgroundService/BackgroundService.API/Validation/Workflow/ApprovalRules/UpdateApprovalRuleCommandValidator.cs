using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.API.Validation.Common;
using BackgroundService.Application.Workflow.ApprovalRules.Commands.UpdateApprovalRule;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRule;
using BackgroundService.Domain.Entities.Workflow;
using FluentValidation;

namespace BackgroundService.API.Validation.Workflow.ApprovalRules
{
    public class UpdateApprovalRuleCommandValidator : AbstractValidator<UpdateApprovalRuleCommand>
    {
        private readonly List<Common.ValidationRule> _validationRules;
        private readonly IApprovalRuleQuery _approvalRuleQuery;
         public UpdateApprovalRuleCommandValidator(MaxLengthProvider maxLengthProvider, IApprovalRuleQuery approvalRuleQuery)
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
                        //     .WithMessage($"{nameof(UpdateApprovalRuleCommand.ConditionKey)} {rule.Error}");
                        // RuleFor(x => x.Operator)
                        //     .NotEmpty()
                        //     .WithMessage($"{nameof(UpdateApprovalRuleCommand.Operator)} {rule.Error}");
                        // RuleFor(x => x.Value)
                        //     .NotEmpty()
                        //     .WithMessage($"{nameof(UpdateApprovalRuleCommand.Value)} {rule.Error}");
                        // RuleFor(x => x.Action)
                        //      .NotEmpty()
                        //      .WithMessage($"{nameof(UpdateApprovalRuleCommand.Action)} {rule.Error}");
                       
                        break;
                    case "MaxLength":
                        // RuleFor(x => x.ConditionKey)
                        //     .MaximumLength(maxLength)
                        //     .WithMessage($"{nameof(UpdateApprovalRuleCommand.ConditionKey)} {rule.Error}");
                        // RuleFor(x => x.Operator)
                        //     .MaximumLength(Operator)
                        //     .WithMessage($"{nameof(UpdateApprovalRuleCommand.Operator)} {rule.Error}");
                        // RuleFor(x => x.Value)
                        //     .MaximumLength(Value)
                        //     .WithMessage($"{nameof(UpdateApprovalRuleCommand.Value)} {rule.Error}");
                        // RuleFor(x => x.Action)
                        //     .MaximumLength(Action)
                        //     .WithMessage($"{nameof(UpdateApprovalRuleCommand.Action)} {rule.Error}");
                        break;
                    case "AlreadyExists":
                        // RuleFor(x => new { x.ConditionKey, x.Operator, x.Value, x.Action, x.UnitId, x.WorkflowTypeId, x.Id })
                        //   .MustAsync(async (approvalType, cancellation) => !await _approvalRuleQuery.AlreadyExistsAsync(approvalType.ConditionKey, approvalType.Operator, approvalType.Value, approvalType.Action, approvalType.UnitId, approvalType.WorkflowTypeId, approvalType.Id))
                        //    .WithName("Approval Rule")
                        //      .WithMessage($"{rule.Error}");
                        break;
                    case "NotFound":
                           RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _approvalRuleQuery.NotFoundAsync(Id))             
                           .WithName("Approval Rule Id")
                            .WithMessage($"{rule.Error}");
                            break; 

                }
            }
         }
    }
}