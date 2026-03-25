using MediatR;
using SalesManagement.Application.Complaint.Dto;

namespace SalesManagement.Application.Complaint.Queries.GetComplaintAutoComplete
{
    public sealed record GetComplaintAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<ComplaintLookupDto>>;
}
