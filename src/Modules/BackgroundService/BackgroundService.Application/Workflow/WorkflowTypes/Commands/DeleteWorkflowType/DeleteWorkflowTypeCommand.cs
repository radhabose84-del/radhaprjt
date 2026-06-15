using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Contracts.Common;

namespace BackgroundService.Application.Workflow.WorkflowTypes.Commands.DeleteWorkflowType
{
    public class DeleteWorkflowTypeCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
