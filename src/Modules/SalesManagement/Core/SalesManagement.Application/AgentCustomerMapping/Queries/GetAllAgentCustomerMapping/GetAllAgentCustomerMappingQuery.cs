using Contracts.Common;
using MediatR;
using SalesManagement.Application.AgentCustomerMapping.Dto;

namespace SalesManagement.Application.AgentCustomerMapping.Queries.GetAllAgentCustomerMapping
{
    public class GetAllAgentCustomerMappingQuery : IRequest<ApiResponseDTO<List<AgentCustomerMappingDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
