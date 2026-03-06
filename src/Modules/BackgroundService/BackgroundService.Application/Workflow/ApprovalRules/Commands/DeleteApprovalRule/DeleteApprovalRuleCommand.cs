using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalRules.Commands.DeleteApprovalRule
{
    public class DeleteApprovalRuleCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}