using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRule;
using BackgroundService.Domain.Entities.Workflow;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalRules.Commands.DeleteApprovalRule
{
    public class DeleteApprovalRuleCommandHandler : IRequestHandler<DeleteApprovalRuleCommand, bool>
    {
        private readonly IApprovalRuleCommand _approvalRuleCommand;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        public DeleteApprovalRuleCommandHandler(IApprovalRuleCommand approvalRuleCommand, IMediator imediator, IMapper imapper)
        {
            _approvalRuleCommand = approvalRuleCommand;
            _imediator = imediator;
            _imapper = imapper;
        }
        public async Task<bool> Handle(DeleteApprovalRuleCommand request, CancellationToken cancellationToken)
        {
            var ApprovalRule = _imapper.Map<ApprovalRule>(request);
            var result = await _approvalRuleCommand.DeleteAsync(request.Id,ApprovalRule);
        
            return result == true ? result : throw new ExceptionRules("Approval Rule deletion failed.");
        }
    }
}