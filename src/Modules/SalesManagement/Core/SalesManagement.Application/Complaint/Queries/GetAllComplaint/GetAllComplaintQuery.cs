using Contracts.Common;
using MediatR;
using SalesManagement.Application.Complaint.Dto;

namespace SalesManagement.Application.Complaint.Queries.GetAllComplaint
{
    public class GetAllComplaintQuery : IRequest<ApiResponseDTO<List<ComplaintHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
    }
}
