using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalStepDetail;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetApprovalStepDetailById
{
    public class GetApprovalStepDetailByIdQueryHandler : IRequestHandler<GetApprovalStepDetailByIdQuery, ApprovalStepDetailByIdDto>
    {
         private readonly IApprovalStepDetailQuery _approvalStepDetailQuery;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public GetApprovalStepDetailByIdQueryHandler(IApprovalStepDetailQuery approvalStepDetailQuery, IMediator mediator, IMapper mapper)
        {
             _approvalStepDetailQuery = approvalStepDetailQuery;
            _mediator = mediator;
            _mapper = mapper;
        }
        public async Task<ApprovalStepDetailByIdDto> Handle(GetApprovalStepDetailByIdQuery request, CancellationToken cancellationToken)
        {
             var result = await _approvalStepDetailQuery.GetByIdAsync(request.Id);     
               
            var ApprovalStep = _mapper.Map<ApprovalStepDetailByIdDto>(result);
            
            return ApprovalStep;
        }
    }
}