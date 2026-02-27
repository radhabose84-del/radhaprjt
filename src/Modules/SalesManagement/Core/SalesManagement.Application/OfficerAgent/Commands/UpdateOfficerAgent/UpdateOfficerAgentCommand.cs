using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.OfficerAgent.Commands.UpdateOfficerAgent
{
    public class UpdateOfficerAgentCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public int AgentId { get; set; }
        public int MarketingOfficerId { get; set; }
        public DateOnly ValidityFrom { get; set; }
        public DateOnly ValidityTo { get; set; }
        public int IsActive { get; set; }
    }
}
