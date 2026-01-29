using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest;
using FluentValidation;

namespace BackgroundService.API.Validation.Workflow.ApprovalRequest
{
    public class ApproveRejectCommandValidator : AbstractValidator<ApproveApprovalRequestCommand>
    {
        public ApproveRejectCommandValidator()
        {
            RuleFor(x => x.ApprovalRequestHeaderId)
                            .NotEmpty()
                            .NotNull()
                            .GreaterThan(0)
                            .WithMessage("ApprovalRequestHeaderId is required.");

            RuleFor(x => x.ModuleTransactionId)
                            .NotEmpty()
                            .NotNull()                            
                            .WithMessage("ApprovalRequestHeaderId is required.");

            When(x => x.ApprovalRequestLine != null, () =>
                 {
                     RuleForEach(x => x.ApprovalRequestLine!).ChildRules(detail =>
                    {
                        
                             detail.RuleFor(d => d.ApprovalRequestLineId)
                             .GreaterThan(0)
                             .WithMessage("ApprovalRequestLineId is required.");
                       

                            detail.RuleFor(d => d.ApprovalRequestHeaderId)
                            .GreaterThan(0)
                            .WithMessage("ApprovalRequestHeaderId is required.");
                       
                             detail.RuleFor(d => d.ModuleLineTransactionId)
                             .GreaterThan(0)
                             .WithMessage("ModuleLineTransactionId is required.");
                        
                    });
                 });
        }
    }
}