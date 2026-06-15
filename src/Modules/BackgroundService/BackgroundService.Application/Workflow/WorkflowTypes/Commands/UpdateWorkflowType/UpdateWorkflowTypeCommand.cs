using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Contracts.Common;

namespace BackgroundService.Application.Workflow.WorkflowTypes.Commands.UpdateWorkflowType
{
    public class UpdateWorkflowTypeCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public int MenuId { get; set; }
        public byte HasLine { get; set; }
        public byte IsMultiselect { get; set; }
        public int? TransactionTypeId { get; set; }
        public byte IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
