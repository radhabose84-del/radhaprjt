using Contracts.Common;
using MediatR;
using SalesManagement.Application.Complaint.Dto;

namespace SalesManagement.Application.Complaint.Queries.GetPendingComplaint
{
    public class GetPendingComplaintQuery : IRequest<ApiResponseDTO<List<ComplaintHeaderDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
