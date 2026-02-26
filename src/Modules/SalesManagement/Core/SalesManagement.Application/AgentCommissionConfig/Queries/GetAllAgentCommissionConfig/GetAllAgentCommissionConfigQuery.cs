using Contracts.Common;
using MediatR;
using SalesManagement.Application.AgentCommissionConfig.Dto;

namespace SalesManagement.Application.AgentCommissionConfig.Queries.GetAllAgentCommissionConfig
{
    public class GetAllAgentCommissionConfigQuery : IRequest<ApiResponseDTO<List<AgentCommissionConfigDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
