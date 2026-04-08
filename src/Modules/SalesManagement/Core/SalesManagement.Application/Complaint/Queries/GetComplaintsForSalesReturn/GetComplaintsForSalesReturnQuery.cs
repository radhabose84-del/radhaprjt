using MediatR;
using SalesManagement.Application.Complaint.Dto;

namespace SalesManagement.Application.Complaint.Queries.GetComplaintsForSalesReturn
{
    public sealed record GetComplaintsForSalesReturnQuery(string Term)
        : IRequest<IReadOnlyList<ComplaintForSalesReturnLookupDto>>;
}
