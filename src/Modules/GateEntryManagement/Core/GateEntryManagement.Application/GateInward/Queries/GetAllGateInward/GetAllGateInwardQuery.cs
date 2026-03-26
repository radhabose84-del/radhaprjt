using Contracts.Common;
using GateEntryManagement.Application.GateInward.Dto;
using MediatR;

namespace GateEntryManagement.Application.GateInward.Queries.GetAllGateInward
{
    public class GetAllGateInwardQuery : IRequest<ApiResponseDTO<List<GateInwardHdrDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
