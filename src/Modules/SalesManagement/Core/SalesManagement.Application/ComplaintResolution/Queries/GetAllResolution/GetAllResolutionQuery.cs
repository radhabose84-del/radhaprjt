using Contracts.Common;
using MediatR;
using SalesManagement.Application.ComplaintResolution.Dto;

namespace SalesManagement.Application.ComplaintResolution.Queries.GetAllResolution
{
    public class GetAllResolutionQuery : IRequest<ApiResponseDTO<List<ResolutionListDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
    }
}
