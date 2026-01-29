using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.API.Validation.Common;
using BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.CreateApprovalStepDetail;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail;
using FluentValidation;

namespace BackgroundService.API.Validation.Workflow.ApprovalStepDetail
{
    public class CreateApprovalStepDetailCommandValidator : AbstractValidator<CreateApprovalStepDetailCommand>
    {
        private readonly List<Common.ValidationRule> _validationRules;
        private readonly IApprovalStepDetailQuery _approvalStepDetailQuery;
        public CreateApprovalStepDetailCommandValidator(IApprovalStepDetailQuery approvalStepDetailQuery)
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
                            .WithMessage($"{nameof(CreateApprovalStepDetailCommand.WorkFlowTypeId)} {rule.Error}")
                            .NotNull()
                            .WithMessage($"{nameof(CreateApprovalStepDetailCommand.WorkFlowTypeId)} {rule.Error}");
                        RuleFor(x => x.StepOrder)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateApprovalStepDetailCommand.StepOrder)} {rule.Error}")
                            .NotNull()
                            .WithMessage($"{nameof(CreateApprovalStepDetailCommand.StepOrder)} {rule.Error}");
                        // RuleFor(x => x.TargetTypeId)
                        //     .NotEmpty()
                        //     .WithMessage($"{nameof(CreateApprovalStepDetailCommand.TargetTypeId)} {rule.Error}")
                        //     .NotNull()
                        //     .WithMessage($"{nameof(CreateApprovalStepDetailCommand.TargetTypeId)} {rule.Error}");
                            RuleFor(x => x.ApprovalStepId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateApprovalStepDetailCommand.ApprovalStepId)} {rule.Error}")
                            .NotNull()
                            .WithMessage($"{nameof(CreateApprovalStepDetailCommand.ApprovalStepId)} {rule.Error}");
                        // RuleFor(x => x.ApprovalTypeId)
                        //     .NotEmpty()
                        //     .WithMessage($"{nameof(CreateApprovalStepDetailCommand.ApprovalTypeId)} {rule.Error}")
                        //     .NotNull()
                        //     .WithMessage($"{nameof(CreateApprovalStepDetailCommand.ApprovalTypeId)} {rule.Error}");
                        break;
                    case "AlreadyExists":                       
                        // RuleFor(x => new { x.WorkFlowTypeId, x.TargetTypeId, x.ApprovalStepId,x.ApprovalTypeId })
                        //   .MustAsync(async (ApprovalStep, cancellation) => !await _approvalStepDetailQuery.AlreadyExistsAsync(ApprovalStep.WorkFlowTypeId,ApprovalStep.TargetTypeId,ApprovalStep.ApprovalStepId,ApprovalStep.ApprovalTypeId))
                        //    .WithName("Approval Step Detail")
                        //      .WithMessage($"{rule.Error}");
                        break;

                }
            }
        }
    }
}