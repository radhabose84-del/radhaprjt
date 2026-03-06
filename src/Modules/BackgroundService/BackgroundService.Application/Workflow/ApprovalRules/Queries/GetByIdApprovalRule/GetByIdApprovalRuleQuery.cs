using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalRules.Queries.GetByIdApprovalRule
{
    public class GetByIdApprovalRuleQuery : IRequest<ApprovalRuleByIdDto>
    {
        
        public int Id { get; set; }
    }
}