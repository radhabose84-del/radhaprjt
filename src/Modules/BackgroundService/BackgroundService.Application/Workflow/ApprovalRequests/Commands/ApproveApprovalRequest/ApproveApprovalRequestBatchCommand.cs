using System.Collections.Generic;
using System.Text.Json;
using Contracts.Common;
using Contracts.Dtos.Common;
using MediatR;

namespace BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest
{
    public class ApproveApprovalRequestBatchCommand : IRequest<ApproveBatchResultDto>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanApprove;
        public List<ApproveApprovalRequestItemDto> Items { get; set; } = new();
    }

    public class ApproveApprovalRequestItemDto
    {
        public int ApprovalRequestHeaderId { get; set; }   
        public int ModuleTransactionId { get; set; }       
        public string Remark { get; set; } = string.Empty;
        public byte? IsApproved { get; set; }

        public ICollection<ApprovalDocumentDto>? ApprovalDocument { get; set; }
        public ICollection<ApproveApprovalRequestLineDto>? ApprovalRequestLine { get; set; }

        public List<PartyRefDto> PartyContacts { get; set; } = new();
        public List<JsonElement> DynamicFields { get; set; } = new();
    }
}
