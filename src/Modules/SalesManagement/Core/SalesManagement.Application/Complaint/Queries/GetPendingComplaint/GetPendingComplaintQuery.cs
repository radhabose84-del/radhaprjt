using MediatR;
using SalesManagement.Application.Complaint.Dto;

namespace SalesManagement.Application.Complaint.Queries.GetPendingComplaint
{
    public class GetPendingComplaintQuery
        : IRequest<(List<PendingComplaintListDto> Items, int TotalCount)>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
