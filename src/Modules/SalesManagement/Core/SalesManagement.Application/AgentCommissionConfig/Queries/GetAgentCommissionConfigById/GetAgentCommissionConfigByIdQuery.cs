using MediatR;
using SalesManagement.Application.AgentCommissionConfig.Dto;

namespace SalesManagement.Application.AgentCommissionConfig.Queries.GetAgentCommissionConfigById
{
    public class GetAgentCommissionConfigByIdQuery : IRequest<AgentCommissionConfigDto?>
    {
        public int Id { get; set; }
    }
}
