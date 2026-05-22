using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.OfficerAgent.Commands.UpdateOfficerAgent
{
    public class UpdateOfficerAgentCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int MarketingOfficerId { get; set; }
        public List<OfficerAgentUpdateItem> Agents { get; set; } = new();
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }

    public class OfficerAgentUpdateItem
    {
        public int Id { get; set; }
        public int AgentId { get; set; }
        public DateOnly ValidityFrom { get; set; }
        public DateOnly ValidityTo { get; set; }
        public int IsActive { get; set; }
    }
}
