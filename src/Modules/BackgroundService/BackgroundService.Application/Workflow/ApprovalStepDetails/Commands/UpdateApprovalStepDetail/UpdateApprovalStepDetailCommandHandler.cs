using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail;
using BackgroundService.Domain.Entities.Workflow;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.UpdateApprovalStepDetail
{
    public class UpdateApprovalStepDetailCommandHandler : IRequestHandler<UpdateApprovalStepDetailCommand, bool>
    {
         private readonly IApprovalStepDetailCommand  _approvalStepDetailCommand;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        public UpdateApprovalStepDetailCommandHandler(IApprovalStepDetailCommand approvalStepDetailCommand, IMediator imediator, IMapper imapper)
        {
            _approvalStepDetailCommand = approvalStepDetailCommand;
            _imediator = imediator;
            _imapper = imapper;
        }
        public async Task<bool> Handle(UpdateApprovalStepDetailCommand request, CancellationToken cancellationToken)
        {
            var WorkflowType = _imapper.Map<ApprovalStepDetail>(request);
            var result = await _approvalStepDetailCommand.UpdateAsync(WorkflowType);
            
           
            return result == true ? result : throw new ExceptionRules("Approval Step Detail update failed.");  
        }
    }
}