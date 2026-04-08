using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Complaint.Queries.GetComplaintsForSalesReturn
{
    public class GetComplaintsForSalesReturnQueryHandler
        : IRequestHandler<GetComplaintsForSalesReturnQuery, IReadOnlyList<ComplaintForSalesReturnLookupDto>>
    {
        private readonly IComplaintQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetComplaintsForSalesReturnQueryHandler(
            IComplaintQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<ComplaintForSalesReturnLookupDto>> Handle(
            GetComplaintsForSalesReturnQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetComplaintsForSalesReturnAsync(
                request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetComplaintsForSalesReturnQuery",
                actionName: result.Count.ToString(),
                details: "Complaints eligible for Sales Return were fetched.",
                module: "Complaint");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
