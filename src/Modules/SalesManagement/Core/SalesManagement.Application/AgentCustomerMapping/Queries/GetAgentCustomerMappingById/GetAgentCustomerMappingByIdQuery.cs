using MediatR;
using SalesManagement.Application.AgentCustomerMapping.Dto;

namespace SalesManagement.Application.AgentCustomerMapping.Queries.GetAgentCustomerMappingById
{
    public class GetAgentCustomerMappingByIdQuery : IRequest<AgentCustomerMappingDto?>
    {
        public int Id { get; set; }
    }
}
