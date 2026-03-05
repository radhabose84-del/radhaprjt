using Contracts.Common;
using MediatR;
using SalesManagement.Application.OfficerAgent.Dto;

namespace SalesManagement.Application.OfficerAgent.Queries.GetAllOfficerAgent
{
    public class GetAllOfficerAgentQuery : IRequest<ApiResponseDTO<List<OfficerAgentGroupedDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
