using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.API.Validation.Common;
using BackgroundService.Application.Workflow.ApprovalRules.Commands.DeleteApprovalRule;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRule;
using FluentValidation;

namespace BackgroundService.API.Validation.Workflow.ApprovalRules
{
    public class DeleteApprovalRuleCommandValidator : AbstractValidator<DeleteApprovalRuleCommand>
    {
        private readonly List<Common.ValidationRule> _validationRules;
        private readonly IApprovalRuleQuery _approvalRuleQuery;
        public DeleteApprovalRuleCommandValidator(IApprovalRuleQuery approvalRuleQuery)
        {
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
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteApprovalRuleCommand.Id)} {rule.Error}");
                        break;
                        case "NotFound":
                           RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _approvalRuleQuery.NotFoundAsync(Id))             
                           .WithName("Approval Rule Id")
                            .WithMessage($"{rule.Error}");
                            break; 
                    default:
                        
                        break;
                }
            }
        }
    }
}