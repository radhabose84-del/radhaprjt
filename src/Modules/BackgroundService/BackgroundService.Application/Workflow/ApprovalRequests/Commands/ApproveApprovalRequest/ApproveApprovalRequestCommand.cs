using System.ComponentModel;
using System.Text.Json;
using Contracts.Common;
using Contracts.Dtos.Common;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest
{
    public class ApproveApprovalRequestCommand : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanApprove;
        public int ApprovalRequestHeaderId { get; set; }
        // public int WorkFlowTypeId { get; set; }
           public int ModuleTransactionId { get; set; }   
        // public string ModuleTypeName { get; set; }
        // public int UnitId { get; set; }
        // public int DepartmentId { get; set; }
        public string Remark { get; set; }
        public byte? IsApproved { get; set; }
        public ICollection<ApprovalDocumentDto>? ApprovalDocument { get; set; }
        public ICollection<ApproveApprovalRequestLineDto>? ApprovalRequestLine { get; set; }
        public List<PartyRefDto> PartyContacts { get; set; } = new();
        public List<JsonElement> DynamicFields { get; set; } = new();
        
    }
}