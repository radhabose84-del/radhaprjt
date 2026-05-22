using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Contracts.Common;

namespace BackgroundService.Application.Workflow.WorkflowTypes.Commands.CreateWorkflowType
{
    public class CreateWorkflowTypeCommand : IRequest<int>, IRequirePermission
    {
        public int ModuleId { get; set; }
        public int MenuId { get; set; }
        public byte HasLine { get; set; }
        public byte IsMultiselect { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
