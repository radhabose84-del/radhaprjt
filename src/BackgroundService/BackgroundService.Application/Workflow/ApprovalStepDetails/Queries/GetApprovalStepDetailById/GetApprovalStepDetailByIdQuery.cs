using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalStepDetails.Queries.GetApprovalStepDetailById
{
    public class GetApprovalStepDetailByIdQuery : IRequest<ApprovalStepDetailByIdDto>
    {
         public int Id { get; set; }
        
    }
}