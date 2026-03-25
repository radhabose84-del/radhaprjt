using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.Complaint.Queries.GetComplaintById
{
    public class GetComplaintByIdQueryHandler : IRequestHandler<GetComplaintByIdQuery, ComplaintHeaderDto?>
    {
        private readonly IComplaintQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetComplaintByIdQueryHandler(IComplaintQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ComplaintHeaderDto?> Handle(GetComplaintByIdQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetByIdAsync(request.Id);

            if (data == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetComplaintByIdQuery",
                actionName: data.Id.ToString(),
                details: $"Complaint details {data.Id} was fetched.",
                module: "Complaint");
            await _mediator.Publish(domainEvent, cancellationToken);

            return data;
        }
    }
}
