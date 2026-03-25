using Contracts.Common;
using GateEntryManagement.Application.GatePass.Dto;
using MediatR;

namespace GateEntryManagement.Application.GatePass.Queries.GetAllGatePass
{
    public class GetAllGatePassQuery : IRequest<ApiResponseDTO<List<GatePassHdrDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
