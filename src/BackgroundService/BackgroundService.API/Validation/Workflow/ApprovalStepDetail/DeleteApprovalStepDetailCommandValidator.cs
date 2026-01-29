using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.API.Validation.Common;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.DeleteApprovalStepDetail;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail;
using FluentValidation;

namespace BackgroundService.API.Validation.Workflow.ApprovalStepDetail
{
    public class DeleteApprovalStepDetailCommandValidator : AbstractValidator<DeleteApprovalStepDetailCommand>
    {
        private readonly List<Common.ValidationRule> _validationRules;
        private readonly IApprovalStepDetailQuery _approvalStepDetailQuery;
        public DeleteApprovalStepDetailCommandValidator(IApprovalStepDetailQuery approvalStepDetailQuery)
        {
            _approvalStepDetailQuery = approvalStepDetailQuery;
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
                            .WithMessage($"{nameof(DeleteApprovalStepDetailCommand.Id)} {rule.Error}");
                        break;
                        case "NotFound":
                           RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _approvalStepDetailQuery.NotFoundAsync(Id))             
                           .WithName("Approval Step Id")
                            .WithMessage($"{rule.Error}");
                            break; 
                    default:
                        
                        break;
                }
            }
        }
    }
}