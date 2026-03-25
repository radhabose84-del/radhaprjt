using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Complaint.Queries.GetComplaintAutoComplete
{
    public class GetComplaintAutoCompleteQueryHandler : IRequestHandler<GetComplaintAutoCompleteQuery, IReadOnlyList<ComplaintLookupDto>>
    {
        private readonly IComplaintQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetComplaintAutoCompleteQueryHandler(IComplaintQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<ComplaintLookupDto>> Handle(GetComplaintAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetComplaintAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "Complaint details was fetched.",
                module: "Complaint");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
