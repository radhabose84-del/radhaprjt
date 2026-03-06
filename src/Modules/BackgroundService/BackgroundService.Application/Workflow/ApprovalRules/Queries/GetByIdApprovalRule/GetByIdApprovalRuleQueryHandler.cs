using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackgroundService.Application.Workflow.Common.Interfaces.IApprovalRule;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalRules.Queries.GetByIdApprovalRule
{
    public class GetByIdApprovalRuleQueryHandler : IRequestHandler<GetByIdApprovalRuleQuery, ApprovalRuleByIdDto>
    {
        private readonly IApprovalRuleQuery _approvalRuleQuery;
        private readonly IMapper _mapper;
        public GetByIdApprovalRuleQueryHandler(IApprovalRuleQuery approvalRuleQuery, IMapper mapper)
        {
            _approvalRuleQuery = approvalRuleQuery;
            _mapper = mapper;
        }
        public async Task<ApprovalRuleByIdDto> Handle(GetByIdApprovalRuleQuery request, CancellationToken cancellationToken)
        {
            var result = await _approvalRuleQuery.GetByIdAsync(request.Id);     
               
            var ApprovalRule = _mapper.Map<ApprovalRuleByIdDto>(result);
            
            return ApprovalRule;
        }
    }
}