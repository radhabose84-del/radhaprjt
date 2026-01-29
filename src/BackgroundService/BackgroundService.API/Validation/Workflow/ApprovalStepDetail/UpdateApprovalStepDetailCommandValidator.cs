using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.API.Validation.Common;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.UpdateApprovalStepDetail;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail;
using FluentValidation;

namespace BackgroundService.API.Validation.Workflow.ApprovalStepDetail
{
    public class UpdateApprovalStepDetailCommandValidator : AbstractValidator<UpdateApprovalStepDetailCommand>
    {
         private readonly List<Common.ValidationRule> _validationRules;
        private readonly IApprovalStepDetailQuery _approvalStepDetailQuery;
        public UpdateApprovalStepDetailCommandValidator(IApprovalStepDetailQuery approvalStepDetailQuery)
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
                        RuleFor(x => x.WorkFlowTypeId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateApprovalStepDetailCommand.WorkFlowTypeId)} {rule.Error}")
                            .NotNull()
                            .WithMessage($"{nameof(UpdateApprovalStepDetailCommand.WorkFlowTypeId)} {rule.Error}");
                        RuleFor(x => x.StepOrder)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateApprovalStepDetailCommand.StepOrder)} {rule.Error}")
                            .NotNull()
                            .WithMessage($"{nameof(UpdateApprovalStepDetailCommand.StepOrder)} {rule.Error}");
                        // RuleFor(x => x.TargetTypeId)
                        //     .NotEmpty()
                        //     .WithMessage($"{nameof(UpdateApprovalStepDetailCommand.TargetTypeId)} {rule.Error}")
                        //     .NotNull()
                        //     .WithMessage($"{nameof(UpdateApprovalStepDetailCommand.TargetTypeId)} {rule.Error}");
                        RuleFor(x => x.ApprovalStepId)
                        .NotEmpty()
                        .WithMessage($"{nameof(UpdateApprovalStepDetailCommand.ApprovalStepId)} {rule.Error}")
                        .NotNull()
                        .WithMessage($"{nameof(UpdateApprovalStepDetailCommand.ApprovalStepId)} {rule.Error}");
                        // RuleFor(x => x.ApprovalTypeId)
                        //     .NotEmpty()
                        //     .WithMessage($"{nameof(UpdateApprovalStepDetailCommand.ApprovalTypeId)} {rule.Error}")
                        //     .NotNull()
                        //     .WithMessage($"{nameof(UpdateApprovalStepDetailCommand.ApprovalTypeId)} {rule.Error}");
                        break;
                    case "AlreadyExists":
                        // RuleFor(x => new { x.WorkFlowTypeId, x.TargetTypeId, x.ApprovalStepId, x.ApprovalTypeId, x.Id })
                        //   .MustAsync(async (ApprovalStep, cancellation) => !await _approvalStepDetailQuery.AlreadyExistsAsync(
                        //     ApprovalStep.WorkFlowTypeId, ApprovalStep.TargetTypeId, ApprovalStep.ApprovalStepId, ApprovalStep.ApprovalTypeId, ApprovalStep.Id))
                        //    .WithName("Approval Step Detail")
                        //      .WithMessage($"{rule.Error}");
                        break;
                        
                     case "NotFound":
                           RuleFor(x => x.Id )
                           .MustAsync(async (Id, cancellation) => 
                        await _approvalStepDetailQuery.NotFoundAsync(Id))             
                           .WithName("Approval Step Id")
                            .WithMessage($"{rule.Error}");
                            break; 

                }
            }
        }
    }
}