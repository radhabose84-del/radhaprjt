using Contracts.Common;
using MediatR;
using SalesManagement.Application.ComplaintResolution.Dto;

namespace SalesManagement.Application.ComplaintResolution.Queries.GetResolutionFormData
{
    public class GetResolutionFormDataQuery : IRequest<ApiResponseDTO<ComplaintResolutionFormDataDto>>
    {
        public int ComplaintHeaderId { get; set; }
    }
}
