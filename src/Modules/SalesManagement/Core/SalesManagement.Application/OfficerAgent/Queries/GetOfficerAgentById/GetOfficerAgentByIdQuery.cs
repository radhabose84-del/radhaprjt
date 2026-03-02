using Contracts.Common;
using MediatR;
using SalesManagement.Application.OfficerAgent.Dto;

namespace SalesManagement.Application.OfficerAgent.Queries.GetOfficerAgentById
{
    public class GetOfficerAgentByIdQuery : IRequest<ApiResponseDTO<OfficerAgentDto>>
    {
        public int Id { get; set; }
    }
}
