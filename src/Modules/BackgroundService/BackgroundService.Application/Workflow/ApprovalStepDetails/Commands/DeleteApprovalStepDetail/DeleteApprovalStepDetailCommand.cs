using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalStepDetails.Commands.DeleteApprovalStepDetail
{
    public class DeleteApprovalStepDetailCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }
}