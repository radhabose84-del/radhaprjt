using MediatR;
using SalesManagement.Application.Complaint.Dto;

namespace SalesManagement.Application.Complaint.Queries.GetComplaintById
{
    public class GetComplaintByIdQuery : IRequest<ComplaintHeaderDto?>
    {
        public int Id { get; set; }
    }
}
