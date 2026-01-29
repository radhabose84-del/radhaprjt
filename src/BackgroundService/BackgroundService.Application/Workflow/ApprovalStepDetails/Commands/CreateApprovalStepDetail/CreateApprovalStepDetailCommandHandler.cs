using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Notification.Exceptions;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail;
using BackgroundService.Domain.Entities.Workflow;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.CreateApprovalStepDetail
{
    public class CreateApprovalStepDetailCommandHandler : IRequestHandler<CreateApprovalStepDetailCommand, int>
    {
        private readonly IApprovalStepDetailCommand _approvalStepDetailCommand;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        public CreateApprovalStepDetailCommandHandler(IApprovalStepDetailCommand approvalStepDetailCommand, IMediator imediator, IMapper imapper)
        {
             _approvalStepDetailCommand= approvalStepDetailCommand;
            _imediator = imediator;
            _imapper = imapper;
        }
        public async Task<int> Handle(CreateApprovalStepDetailCommand request, CancellationToken cancellationToken)
        {
            var ApprovalStep = _imapper.Map<ApprovalStepDetail>(request);
            
            var result = await _approvalStepDetailCommand.CreateAsync(ApprovalStep);
            
            return result > 0 ? result : throw new ExceptionRules("Approval Step Detail Creation Failed.");
        }
    }
}