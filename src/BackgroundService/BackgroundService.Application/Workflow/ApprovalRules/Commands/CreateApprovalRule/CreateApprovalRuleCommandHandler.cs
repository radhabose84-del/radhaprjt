using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRule;
using BackgroundService.Domain.Entities.Workflow;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalRules.Commands.CreateApprovalRule
{
    public class CreateApprovalRuleCommandHandler : IRequestHandler<CreateApprovalRuleCommand, int>
    {
        private readonly IApprovalRuleCommand _approvalRuleCommand;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        public CreateApprovalRuleCommandHandler(IApprovalRuleCommand approvalRuleCommand, IMediator imediator, IMapper imapper)
        {
            _approvalRuleCommand = approvalRuleCommand;
            _imediator = imediator;
            _imapper = imapper;
        }
        public async Task<int> Handle(CreateApprovalRuleCommand request, CancellationToken cancellationToken)
        {
            var ApprovalRule = _imapper.Map<ApprovalRule>(request);
            
            var result = await _approvalRuleCommand.CreateAsync(ApprovalRule);
            
            return result > 0 ? result : throw new ExceptionRules("Approval Rule Creation Failed.");
        }
    }
}