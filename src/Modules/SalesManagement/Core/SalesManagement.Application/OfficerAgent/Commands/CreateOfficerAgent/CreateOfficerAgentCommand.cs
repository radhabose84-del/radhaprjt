using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.OfficerAgent.Commands.CreateOfficerAgent
{
    public class CreateOfficerAgentCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int MarketingOfficerId { get; set; }
        public List<OfficerAgentBatchItem> Agents { get; set; } = new();
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }

    public class OfficerAgentBatchItem
    {
        public int AgentId { get; set; }
        public DateOnly ValidityFrom { get; set; }
        public DateOnly ValidityTo { get; set; }
        public int IsActive { get; set; } = 1;
    }
}
