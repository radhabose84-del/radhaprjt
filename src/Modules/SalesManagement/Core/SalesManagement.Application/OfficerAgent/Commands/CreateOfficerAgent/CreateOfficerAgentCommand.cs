using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.OfficerAgent.Commands.CreateOfficerAgent
{
    public class CreateOfficerAgentCommand : IRequest<ApiResponseDTO<int>>
    {
        public int AgentId { get; set; }
        public int MarketingOfficerId { get; set; }
        public DateOnly ValidityFrom { get; set; }
        public DateOnly ValidityTo { get; set; }
    }
}
