using Contracts.Common;
using MediatR;
using SalesManagement.Application.AgentPortal.Dto;

namespace SalesManagement.Application.AgentPortal.Queries.GetAgentDashboard
{
    public class GetAgentDashboardQuery : IRequest<ApiResponseDTO<AgentDashboardDto>> { }
}
