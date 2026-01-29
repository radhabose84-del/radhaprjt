using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRule;
using BackgroundService.Domain.Entities.Workflow;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalRules.Commands.UpdateApprovalRule
{
    public class UpdateApprovalRuleCommandHandler : IRequestHandler<UpdateApprovalRuleCommand, bool>
    {
        private readonly IApprovalRuleCommand _approvalRuleCommand;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        public UpdateApprovalRuleCommandHandler(IApprovalRuleCommand approvalRuleCommand, IMediator imediator, IMapper imapper)
        {
            _approvalRuleCommand = approvalRuleCommand;
            _imediator = imediator;
            _imapper = imapper;
        }
        public async Task<bool> Handle(UpdateApprovalRuleCommand request, CancellationToken cancellationToken)
        {
            var ApprovalRule = _imapper.Map<ApprovalRule>(request);
             await _approvalRuleCommand.UpdateAsync(ApprovalRule);
            
           
            return true;  
        }
    }
}