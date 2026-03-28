using MediatR;
using SalesManagement.Application.ComplaintResolution.Dto;

namespace SalesManagement.Application.ComplaintResolution.Queries.GetResolutionByComplaintId
{
    public class GetResolutionByComplaintIdQuery : IRequest<ComplaintResolutionDto?>
    {
        public int ComplaintHeaderId { get; set; }
    }
}
